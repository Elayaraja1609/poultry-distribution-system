import React, { useState, useEffect } from 'react';
import inventoryService, { StockMovement, CreateStockMovementDto } from '../services/inventoryService';
import farmService from '../services/farmService';
import chickenService from '../services/chickenService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './StockMovements.css';

const StockMovements: React.FC = () => {
  const [movements, setMovements] = useState<StockMovement[]>([]);
  const [farms, setFarms] = useState<any[]>([]);
  const [chickens, setChickens] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState({
    farmId: '',
    chickenId: '',
    startDate: '',
    endDate: '',
  });
  const [formData, setFormData] = useState<CreateStockMovementDto>({
    farmId: '',
    chickenId: '',
    movementType: 'Adjustment',
    quantity: 0,
    reason: '',
  });

  useEffect(() => {
    loadFarms();
    loadChickens();
  }, []);

  useEffect(() => {
    loadMovements();
  }, [pageNumber, filters]);

  const loadFarms = async () => {
    try {
      const response = await farmService.getAll(1, 100);
      if (response.success && response.data) {
        setFarms(response.data.items);
      }
    } catch (err: any) {
      console.error('Failed to load farms:', err);
    }
  };

  const loadChickens = async () => {
    try {
      const response = await chickenService.getAll(1, 100);
      if (response.success && response.data) {
        setChickens(response.data.items);
      }
    } catch (err: any) {
      console.error('Failed to load chickens:', err);
    }
  };

  const loadMovements = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await inventoryService.getStockMovements(
        filters.farmId || undefined,
        filters.chickenId || undefined,
        filters.startDate || undefined,
        filters.endDate || undefined,
        pageNumber,
        10
      );
      if (response.success && response.data) {
        setMovements(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load stock movements');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setError('');
      await inventoryService.recordStockMovement(formData);
      setShowModal(false);
      resetForm();
      loadMovements();
    } catch (err: any) {
      setError(err.message || 'Failed to record stock movement');
    }
  };

  const resetForm = () => {
    setFormData({
      farmId: '',
      chickenId: '',
      movementType: 'Adjustment',
      quantity: 0,
      reason: '',
    });
  };

  const getMovementTypeBadgeClass = (type: string) => {
    const typeMap: { [key: string]: string } = {
      'In': 'success',
      'Out': 'info',
      'Loss': 'error',
      'Adjustment': 'warning',
      'Inbound': 'success',
      'Outbound': 'info',
    };
    return typeMap[type] || 'pending';
  };

  const exportToCSV = () => {
    const headers = ['Date', 'Farm', 'Batch', 'Type', 'Quantity', 'Previous', 'New', 'Reason'];
    const rows = movements.map(m => [
      new Date(m.movementDate).toLocaleDateString(),
      m.farmName,
      m.batchNumber,
      m.movementType,
      m.quantity.toString(),
      m.previousQuantity.toString(),
      m.newQuantity.toString(),
      m.reason || '',
    ]);

    const csv = [headers, ...rows].map(row => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `stock-movements-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
  };

  if (loading && movements.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="stock-movements-page">
      <div className="page-header">
        <h1>Stock Movements</h1>
        <div className="header-actions">
          <button className="btn-secondary" onClick={exportToCSV}>
            Export CSV
          </button>
          <button className="btn-primary" onClick={() => setShowModal(true)}>
            Record Adjustment
          </button>
        </div>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <div className="filters">
        <select
          value={filters.farmId}
          onChange={(e) => setFilters({ ...filters, farmId: e.target.value })}
        >
          <option value="">All Farms</option>
          {farms.map((farm) => (
            <option key={farm.id} value={farm.id}>
              {farm.name}
            </option>
          ))}
        </select>
        <select
          value={filters.chickenId}
          onChange={(e) => setFilters({ ...filters, chickenId: e.target.value })}
        >
          <option value="">All Batches</option>
          {chickens.map((chicken) => (
            <option key={chicken.id} value={chicken.id}>
              {chicken.batchNumber}
            </option>
          ))}
        </select>
        <input
          type="date"
          value={filters.startDate}
          onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
          placeholder="Start Date"
        />
        <input
          type="date"
          value={filters.endDate}
          onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
          placeholder="End Date"
        />
      </div>

      <table className="movements-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Farm</th>
            <th>Batch</th>
            <th>Type</th>
            <th>Quantity</th>
            <th>Previous</th>
            <th>New</th>
            <th>Reason</th>
          </tr>
        </thead>
        <tbody>
          {movements.map((movement) => (
            <tr key={movement.id}>
              <td>{new Date(movement.movementDate).toLocaleDateString()}</td>
              <td>{movement.farmName}</td>
              <td>{movement.batchNumber}</td>
              <td>
                <span className={`status-badge ${getMovementTypeBadgeClass(movement.movementType)}`}>
                  {movement.movementType}
                </span>
              </td>
              <td>{movement.quantity}</td>
              <td>{movement.previousQuantity}</td>
              <td>{movement.newQuantity}</td>
              <td>{movement.reason || '-'}</td>
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

      {showModal && (
        <div className="modal-overlay" onClick={() => { setShowModal(false); resetForm(); }}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>Record Stock Adjustment</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Farm *</label>
                <select
                  value={formData.farmId}
                  onChange={(e) => setFormData({ ...formData, farmId: e.target.value })}
                  required
                >
                  <option value="">Select Farm</option>
                  {farms.map((farm) => (
                    <option key={farm.id} value={farm.id}>
                      {farm.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Chicken Batch *</label>
                <select
                  value={formData.chickenId}
                  onChange={(e) => setFormData({ ...formData, chickenId: e.target.value })}
                  required
                >
                  <option value="">Select Batch</option>
                  {chickens.map((chicken) => (
                    <option key={chicken.id} value={chicken.id}>
                      {chicken.batchNumber}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Movement Type *</label>
                <select
                  value={formData.movementType}
                  onChange={(e) => setFormData({ ...formData, movementType: e.target.value })}
                  required
                >
                  <option value="In">In</option>
                  <option value="Out">Out</option>
                  <option value="Loss">Loss</option>
                  <option value="Adjustment">Adjustment</option>
                </select>
              </div>
              <div className="form-group">
                <label>Quantity *</label>
                <input
                  type="number"
                  value={formData.quantity || ''}
                  onChange={(e) => setFormData({ ...formData, quantity: parseInt(e.target.value) || 0 })}
                  required
                  min="1"
                />
              </div>
              <div className="form-group">
                <label>Reason</label>
                <textarea
                  value={formData.reason}
                  onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                  rows={3}
                />
              </div>
              <div className="modal-actions">
                <button type="submit" className="btn-primary">
                  Record
                </button>
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => { setShowModal(false); resetForm(); }}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default StockMovements;
