import React, { useState, useEffect } from 'react';
import salesService, { Sale } from '../services/salesService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Sales.css';

const Sales: React.FC = () => {
  const [sales, setSales] = useState<Sale[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    loadSales();
  }, [pageNumber]);

  const loadSales = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await salesService.getAll(pageNumber, 10);
      if (response.success && response.data) {
        setSales(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load sales');
    } finally {
      setLoading(false);
    }
  };

  const getPaymentStatusBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      'Pending': 'warning',
      'Partial': 'info',
      'Paid': 'success',
    };
    return statusMap[status] || 'pending';
  };

  if (loading && sales.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="sales-page">
      <div className="page-header">
        <h1>Sales & Payments</h1>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <table className="sales-table">
        <thead>
          <tr>
            <th>Shop</th>
            <th>Sale Date</th>
            <th>Total Amount</th>
            <th>Paid Amount</th>
            <th>Remaining</th>
            <th>Payment Status</th>
            <th>Payment Method</th>
          </tr>
        </thead>
        <tbody>
          {sales.map((sale) => (
            <tr key={sale.id}>
              <td>{sale.shopName}</td>
              <td>{new Date(sale.saleDate).toLocaleDateString()}</td>
              <td>${sale.totalAmount.toFixed(2)}</td>
              <td>${sale.paidAmount.toFixed(2)}</td>
              <td>${sale.remainingAmount.toFixed(2)}</td>
              <td>
                <span className={`status-badge ${getPaymentStatusBadgeClass(sale.paymentStatus)}`}>
                  {sale.paymentStatus}
                </span>
              </td>
              <td>{sale.paymentMethod}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <button
          disabled={pageNumber === 1}
          onClick={() => setPageNumber(pageNumber - 1)}
        >
          Previous
        </button>
        <span>Page {pageNumber} of {totalPages}</span>
        <button
          disabled={pageNumber === totalPages}
          onClick={() => setPageNumber(pageNumber + 1)}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default Sales;
