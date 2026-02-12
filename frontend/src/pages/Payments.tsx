import React, { useState, useEffect } from 'react';
import salesService, { Sale } from '../services/salesService';
import paymentService, { Payment } from '../services/paymentService';
import StripePaymentForm from '../components/StripePaymentForm';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Payments.css';

const Payments: React.FC = () => {
  const [sales, setSales] = useState<Sale[]>([]);
  const [payments, setPayments] = useState<{ [saleId: string]: Payment[] }>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedSale, setSelectedSale] = useState<Sale | null>(null);
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [paymentIntent, setPaymentIntent] = useState<{ clientSecret: string; paymentIntentId: string } | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    loadSales();
  }, [pageNumber]);

  const loadSales = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await salesService.getMySales(pageNumber, 10);
      if (response.success && response.data) {
        setSales(response.data.items);
        setTotalPages(response.data.totalPages ?? 1);
        for (const sale of response.data.items) {
          await loadPaymentsForSale(sale.id);
        }
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load sales');
    } finally {
      setLoading(false);
    }
  };

  const loadPaymentsForSale = async (saleId: string) => {
    try {
      const response = await paymentService.getBySale(saleId);
      if (response.success && response.data) {
        setPayments(prev => ({
          ...prev,
          [saleId]: response.data || [],
        }));
      }
    } catch (err) {
      console.error('Failed to load payments for sale:', err);
    }
  };

  const handlePayNow = async (sale: Sale) => {
    try {
      setError('');
      setSelectedSale(sale);
      const response = await paymentService.createPaymentIntent(sale.remainingAmount, sale.id);
      if (response.success && response.data) {
        setPaymentIntent({
          clientSecret: response.data.clientSecret,
          paymentIntentId: response.data.paymentIntentId,
        });
        setShowPaymentForm(true);
      } else {
        setError(response.message || 'Failed to create payment intent');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to initiate payment');
    }
  };

  const handlePaymentSuccess = () => {
    setShowPaymentForm(false);
    setSelectedSale(null);
    setPaymentIntent(null);
    loadSales();
  };

  const handlePaymentCancel = () => {
    setShowPaymentForm(false);
    setSelectedSale(null);
    setPaymentIntent(null);
  };

  const getPaymentStatusColor = (status: string) => {
    const colors: { [key: string]: string } = {
      'Pending': 'warning',
      'Partial': 'info',
      'Paid': 'success',
    };
    return colors[status] || 'pending';
  };

  if (loading && sales.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="payments-page">
      <div className="page-header-section">
        <div className="page-title-section">
          <h1>My Payments</h1>
          <p>View and pay your outstanding sales.</p>
        </div>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <div className="sales-list">
        {sales.map((sale) => (
          <div key={sale.id} className="sale-card">
            <div className="sale-header">
              <div>
                <h3>Sale #{sale.id.substring(0, 8)}</h3>
                <p className="sale-date">Date: {new Date(sale.saleDate).toLocaleDateString()}</p>
              </div>
              <span className={`status-badge ${getPaymentStatusColor(sale.paymentStatus)}`}>
                {sale.paymentStatus}
              </span>
            </div>
            <div className="sale-details">
              <div className="amount-row">
                <span><strong>Total:</strong> ${sale.totalAmount.toFixed(2)}</span>
                <span><strong>Paid:</strong> ${sale.paidAmount.toFixed(2)}</span>
                <span><strong>Remaining:</strong> ${sale.remainingAmount.toFixed(2)}</span>
              </div>
              {payments[sale.id] && payments[sale.id].length > 0 && (
                <div className="payment-history">
                  <strong>Payment History:</strong>
                  <ul>
                    {payments[sale.id].map((payment) => (
                      <li key={payment.id}>
                        ${payment.amount.toFixed(2)} on {new Date(payment.paymentDate).toLocaleDateString()} ({payment.paymentMethod})
                      </li>
                    ))}
                  </ul>
                </div>
              )}
              {sale.remainingAmount > 0 && (
                <div className="sale-actions">
                  <button className="btn-primary" onClick={() => handlePayNow(sale)}>
                    Pay Now
                  </button>
                </div>
              )}
            </div>
          </div>
        ))}
      </div>

      {totalPages > 1 && (
        <div className="pagination-container">
          <button
            disabled={pageNumber === 1}
            onClick={() => setPageNumber(p => p - 1)}
            className="btn-secondary"
          >
            Previous
          </button>
          <span>Page {pageNumber} of {totalPages}</span>
          <button
            disabled={pageNumber === totalPages}
            onClick={() => setPageNumber(p => p + 1)}
            className="btn-secondary"
          >
            Next
          </button>
        </div>
      )}

      {showPaymentForm && selectedSale && paymentIntent && (
        <div className="modal-overlay" onClick={handlePaymentCancel}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>Complete Payment</h2>
            <StripePaymentForm
              amount={selectedSale.remainingAmount}
              saleId={selectedSale.id}
              clientSecret={paymentIntent.clientSecret}
              paymentIntentId={paymentIntent.paymentIntentId}
              onSuccess={handlePaymentSuccess}
              onCancel={handlePaymentCancel}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default Payments;
