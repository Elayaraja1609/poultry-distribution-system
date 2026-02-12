import React, { useState, useEffect } from 'react';
import orderService, { Order, UpdateFulfillmentDto } from '../services/orderService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import SuccessAlert from '../components/SuccessAlert';
import './Orders.css';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showFulfillmentModal, setShowFulfillmentModal] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [fulfillmentData, setFulfillmentData] = useState<UpdateFulfillmentDto>({ items: [] });
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    loadOrders();
  }, [pageNumber, statusFilter]);

  const loadOrders = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await orderService.getAll(undefined, statusFilter || undefined, pageNumber, 10);
      if (response.success && response.data) {
        setOrders(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load orders');
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (order: Order) => {
    try {
      setError('');
      await orderService.approve(order.id);
      setSuccess('Order approved successfully');
      setTimeout(() => setSuccess(''), 3000);
      setShowApproveModal(false);
      loadOrders();
    } catch (err: any) {
      setError(err.message || 'Failed to approve order');
    }
  };

  const handleReject = async () => {
    if (!selectedOrder || !rejectReason.trim()) {
      setError('Please provide a rejection reason');
      return;
    }

    try {
      setError('');
      await orderService.reject(selectedOrder.id, rejectReason);
      setSuccess('Order rejected successfully');
      setTimeout(() => setSuccess(''), 3000);
      setShowRejectModal(false);
      setRejectReason('');
      loadOrders();
    } catch (err: any) {
      setError(err.message || 'Failed to reject order');
    }
  };

  const handleFulfillment = async () => {
    if (!selectedOrder) return;

    try {
      setError('');
      await orderService.updateFulfillment(selectedOrder.id, fulfillmentData);
      setSuccess('Fulfillment updated successfully');
      setTimeout(() => setSuccess(''), 3000);
      setShowFulfillmentModal(false);
      setFulfillmentData({ items: [] });
      loadOrders();
    } catch (err: any) {
      setError(err.message || 'Failed to update fulfillment');
    }
  };

  const openFulfillmentModal = (order: Order) => {
    setSelectedOrder(order);
    setFulfillmentData({
      items: order.items.map(item => ({
        orderItemId: item.id,
        fulfilledQuantity: item.fulfilledQuantity,
      })),
    });
    setShowFulfillmentModal(true);
  };

  if (loading && orders.length === 0) {
    return <LoadingSpinner />;
  }

  const getStatusBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      'Pending': 'warning',
      'Approved': 'info',
      'Rejected': 'error',
      'Processing': 'warning',
      'PartiallyFulfilled': 'warning',
      'Fulfilled': 'success',
      'Cancelled': 'pending',
    };
    return statusMap[status] || 'pending';
  };

  return (
    <div className="orders-page">
      <div className="page-header-section">
        <div className="page-title-section">
          <h1>Orders Management</h1>
          <p>Processing {orders.length} active client requests.</p>
        </div>
        <div className="action-buttons-header">
          <button className="btn-secondary">Export CSV</button>
          <button className="btn-primary">Print Batch</button>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="metrics-row">
        <div className="metric-card-small">
          <div className="metric-title">Pending Orders</div>
          <div className="metric-value">{orders.filter(o => o.status === 'Pending').length}</div>
          <div className="metric-change negative">
            <span>▼</span>
            <span>-15.0% from yesterday</span>
          </div>
        </div>
        <div className="metric-card-small">
          <div className="metric-title">Scheduled Today</div>
          <div className="metric-value">{orders.filter(o => o.status === 'Approved').length}</div>
          <div className="metric-change positive">
            <span>▲</span>
            <span>+6.0% from average</span>
          </div>
        </div>
        <div className="metric-card-small">
          <div className="metric-title">Completion Rate</div>
          <div className="metric-value">
            {orders.length > 0 
              ? ((orders.filter(o => o.status === 'Fulfilled').length / orders.length) * 100).toFixed(1)
              : '0'}%
          </div>
          <div className="metric-change negative">
            <span>▼</span>
            <span>-0.6% variance</span>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="filters-section">
        <div className="filters-row">
          <div className="search-input-wrapper">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Search by Order ID, Customer, or Region..."
              className="search-input"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="filter-select"
          >
            <option value="">All Status</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Processing">Processing</option>
            <option value="PartiallyFulfilled">Partially Fulfilled</option>
            <option value="Fulfilled">Fulfilled</option>
            <option value="Cancelled">Cancelled</option>
          </select>
          <select className="filter-select">
            <option>Last 7 Days</option>
            <option>Last 30 Days</option>
            <option>Last 90 Days</option>
          </select>
        </div>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}
      {success && <SuccessAlert message={success} onClose={() => setSuccess('')} />}

      {/* Orders Table */}
      <div className="orders-table-container">
        <table className="orders-table">
          <thead>
            <tr>
              <th>Customer</th>
              <th>Date</th>
              <th>Quantity</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.length === 0 ? (
              <tr>
                <td colSpan={5} className="empty-state">
                  <div className="empty-state-icon">📋</div>
                  <div className="empty-state-title">No orders found</div>
                  <div className="empty-state-description">Orders will appear here once created</div>
                </td>
              </tr>
            ) : (
              orders.map((order) => (
                <tr key={order.id}>
                  <td>
                    <span className="order-id-link" style={{ cursor: 'pointer' }}>
                      {order.shopName || 'Unknown Shop'}
                    </span>
                  </td>
                  <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                  <td>{order.items.reduce((sum, item) => sum + item.requestedQuantity, 0)} Units</td>
                  <td>
                    <span className={`status-badge ${getStatusBadgeClass(order.status)}`}>
                      {order.status}
                    </span>
                  </td>
                  <td>
                    <div className="table-actions">
                      {order.status === 'Pending' && (
                        <>
                          <button
                            className="btn-action approve"
                            onClick={() => { setSelectedOrder(order); setShowApproveModal(true); }}
                          >
                            Approve
                          </button>
                          <button
                            className="btn-action view"
                            onClick={() => { setSelectedOrder(order); setShowRejectModal(true); }}
                          >
                            View
                          </button>
                        </>
                      )}
                      {order.status === 'Approved' && (
                        <button className="btn-action schedule">Schedule</button>
                      )}
                      {order.status === 'Processing' && (
                        <button
                          className="btn-action dispatch"
                          onClick={() => openFulfillmentModal(order)}
                        >
                          Dispatch
                        </button>
                      )}
                      {order.status === 'Fulfilled' && (
                        <>
                          <button className="btn-action invoice">Invoice</button>
                          <button className="btn-action reorder">Reorder</button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {orders.length > 0 && (
        <div className="pagination-container">
          <div className="pagination-info">
            Showing {((pageNumber - 1) * 10) + 1}-{Math.min(pageNumber * 10, orders.length)} of {orders.length} orders
          </div>
          <div className="pagination">
            <button
              onClick={() => setPageNumber(p => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
            >
              ‹
            </button>
            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
              const page = i + 1;
              return (
                <button
                  key={page}
                  onClick={() => setPageNumber(page)}
                  className={pageNumber === page ? 'active' : ''}
                >
                  {page}
                </button>
              );
            })}
            {totalPages > 5 && <span>...</span>}
            <button
              onClick={() => setPageNumber(p => Math.min(totalPages, p + 1))}
              disabled={pageNumber === totalPages}
            >
              ›
            </button>
          </div>
        </div>
      )}


      {showApproveModal && selectedOrder && (
        <div className="modal-overlay" onClick={() => { setShowApproveModal(false); setSelectedOrder(null); }}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>Approve Order</h2>
            <p>Are you sure you want to approve this order?</p>
            <div className="modal-actions">
              <button className="btn-primary" onClick={() => handleApprove(selectedOrder)}>
                Confirm Approve
              </button>
              <button className="btn-secondary" onClick={() => { setShowApproveModal(false); setSelectedOrder(null); }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {showRejectModal && selectedOrder && (
        <div className="modal-overlay" onClick={() => { setShowRejectModal(false); setSelectedOrder(null); setRejectReason(''); }}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>Reject Order</h2>
            <div className="form-group">
              <label>Rejection Reason *</label>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                rows={3}
                required
                placeholder="Enter reason for rejection"
              />
            </div>
            <div className="modal-actions">
              <button className="btn-primary" onClick={handleReject}>
                Confirm Reject
              </button>
              <button className="btn-secondary" onClick={() => { setShowRejectModal(false); setSelectedOrder(null); setRejectReason(''); }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {showFulfillmentModal && selectedOrder && (
        <div className="modal-overlay" onClick={() => { setShowFulfillmentModal(false); setSelectedOrder(null); }}>
          <div className="modal-content large" onClick={(e) => e.stopPropagation()}>
            <h2>Update Fulfillment</h2>
            <div className="fulfillment-items">
              {selectedOrder.items.map((item) => {
                const fulfillmentItem = fulfillmentData.items.find(fi => fi.orderItemId === item.id);
                return (
                  <div key={item.id} className="fulfillment-item">
                    <div className="item-info">
                      <strong>{item.batchNumber}</strong>
                      <span>Requested: {item.requestedQuantity}</span>
                    </div>
                    <div className="fulfillment-input">
                      <label>Fulfilled Quantity:</label>
                      <input
                        type="number"
                        min="0"
                        max={item.requestedQuantity}
                        value={fulfillmentItem?.fulfilledQuantity || 0}
                        onChange={(e) => {
                          const newItems = fulfillmentData.items.map(fi =>
                            fi.orderItemId === item.id
                              ? { ...fi, fulfilledQuantity: parseInt(e.target.value) || 0 }
                              : fi
                          );
                          setFulfillmentData({ items: newItems });
                        }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
            <div className="modal-actions">
              <button className="btn-primary" onClick={handleFulfillment}>
                Update Fulfillment
              </button>
              <button className="btn-secondary" onClick={() => { setShowFulfillmentModal(false); setSelectedOrder(null); }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Orders;
