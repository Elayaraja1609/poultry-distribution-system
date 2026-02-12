import React, { useState, useEffect } from 'react';
import chickenService, { Chicken, CreateChickenDto, UpdateChickenDto } from '../services/chickenService';
import supplierService from '../services/supplierService';
import farmService from '../services/farmService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Chickens.css';

const Chickens: React.FC = () => {
  const [chickens, setChickens] = useState<Chicken[]>([]);
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [farms, setFarms] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingChicken, setEditingChicken] = useState<Chicken | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [formData, setFormData] = useState<CreateChickenDto>({
    batchNumber: '',
    supplierId: '',
    farmId: undefined,
    purchaseDate: new Date().toISOString().split('T')[0],
    quantity: 0,
    weightKg: 0,
  });

  useEffect(() => {
    loadData();
  }, [pageNumber]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      
      const [chickensResponse, suppliersResponse, farmsResponse] = await Promise.all([
        chickenService.getAll(pageNumber, 10),
        supplierService.getAll(1, 100),
        farmService.getAll(1, 100),
      ]);

      if (chickensResponse.success && chickensResponse.data) {
        setChickens(chickensResponse.data.items);
        setTotalPages(chickensResponse.data.totalPages);
        
      }

      if (suppliersResponse.success && suppliersResponse.data) {
        setSuppliers(suppliersResponse.data.items);
      }

      if (farmsResponse.success && farmsResponse.data) {
        setFarms(farmsResponse.data.items);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setError('');
      if (editingChicken) {
        const updateData: UpdateChickenDto = {
          farmId: formData.farmId,
          ageDays: editingChicken.ageDays,
          weightKg: formData.weightKg,
          status: editingChicken.status,
          healthStatus: editingChicken.healthStatus,
        };
        await chickenService.update(editingChicken.id, updateData);
      } else {
        await chickenService.create(formData);
      }
      setShowModal(false);
      resetForm();
      loadData();
    } catch (err: any) {
      setError(err.message || 'Failed to save chicken batch');
    }
  };

  const handleEdit = (chicken: Chicken) => {
    setEditingChicken(chicken);
    setFormData({
      batchNumber: chicken.batchNumber,
      supplierId: chicken.supplierId,
      farmId: chicken.farmId,
      purchaseDate: chicken.purchaseDate.split('T')[0],
      quantity: chicken.quantity,
      weightKg: chicken.weightKg,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this chicken batch?')) {
      try {
        setError('');
        await chickenService.delete(id);
        loadData();
      } catch (err: any) {
        setError(err.message || 'Failed to delete chicken batch');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      batchNumber: '',
      supplierId: '',
      farmId: undefined,
      purchaseDate: new Date().toISOString().split('T')[0],
      quantity: 0,
      weightKg: 0,
    });
    setEditingChicken(null);
  };

  const getStatusBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      'Purchased': 'info',
      'InFarm': 'success',
      'ReadyForDistribution': 'warning',
      'InTransit': 'info',
      'Delivered': 'pending',
    };
    return statusMap[status] || 'pending';
  };
  const getHealthBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      '1': 'Healthy',
      '2': 'Sick',
      '3': 'Recovered',
    };
    return statusMap[status] || 'pending';
  };
  if (loading && chickens.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="chickens-page">
      <div className="page-header">
        <h1>Chickens Management</h1>
        <button className="btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          Add Chicken Batch
        </button>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <div className="chickens-table-container">
        <table className="chickens-table">
          <thead>
            <tr>
              <th>Batch Number</th>
              <th>Supplier</th>
              <th>Farm</th>
              <th>Quantity</th>
              <th>Age (Days)</th>
              <th>Weight (Kg)</th>
              <th>Status</th>
              <th>Health</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {chickens.map((chicken) => (
              <tr key={chicken.id}>
                <td>{chicken.batchNumber}</td>
                <td>{chicken.supplierName}</td>
                <td>{chicken.farmName || 'Not Assigned'}</td>
                <td>{chicken.quantity}</td>
                <td>{chicken.ageDays}</td>
                <td>{chicken.weightKg.toFixed(2)}</td>
                <td>
                  <span className={`status-badge ${getStatusBadgeClass(chicken.status)}`}>
                    {chicken.status}
                  </span>
                </td>
                <td>
                  <span className={`health-badge ${getHealthBadgeClass(chicken.healthStatus).toLocaleLowerCase()}`}>
                    {getHealthBadgeClass(chicken.healthStatus)}
                  </span>
                </td>
                <td>
                  <button className="btn-edit" onClick={() => handleEdit(chicken)}>
                    Edit
                  </button>
                  <button className="btn-delete" onClick={() => handleDelete(chicken.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

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
            <h2>{editingChicken ? 'Edit Chicken Batch' : 'Add Chicken Batch'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Batch Number *</label>
                <input
                  type="text"
                  value={formData.batchNumber}
                  onChange={(e) => setFormData({ ...formData, batchNumber: e.target.value })}
                  required
                  disabled={!!editingChicken}
                />
              </div>
              <div className="form-group">
                <label>Supplier *</label>
                <select
                  value={formData.supplierId}
                  onChange={(e) => setFormData({ ...formData, supplierId: e.target.value })}
                  required
                  disabled={!!editingChicken}
                >
                  <option value="">Select Supplier</option>
                  {suppliers.map((supplier) => (
                    <option key={supplier.id} value={supplier.id}>
                      {supplier.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Farm</label>
                <select
                  value={formData.farmId || ''}
                  onChange={(e) => setFormData({ ...formData, farmId: e.target.value || undefined })}
                >
                  <option value="">Select Farm (Optional)</option>
                  {farms.map((farm) => (
                    <option key={farm.id} value={farm.id}>
                      {farm.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Purchase Date *</label>
                <input
                  type="date"
                  value={formData.purchaseDate}
                  onChange={(e) => setFormData({ ...formData, purchaseDate: e.target.value })}
                  required
                  disabled={!!editingChicken}
                />
              </div>
              <div className="form-group">
                <label>Quantity *</label>
                <input
                  type="number"
                  value={formData.quantity}
                  onChange={(e) => setFormData({ ...formData, quantity: parseInt(e.target.value) || 0 })}
                  required
                  min="1"
                  disabled={!!editingChicken}
                />
              </div>
              <div className="form-group">
                <label>Weight (Kg) *</label>
                <input
                  type="number"
                  step="0.01"
                  value={formData.weightKg}
                  onChange={(e) => setFormData({ ...formData, weightKg: parseFloat(e.target.value) || 0 })}
                  required
                  min="0"
                />
              </div>
              <div className="modal-actions">
                <button type="submit" className="btn-primary">
                  {editingChicken ? 'Update' : 'Create'}
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

export default Chickens;
