import React, { useState, useEffect } from 'react';
import expenseService, { Expense, CreateExpenseDto } from '../services/expenseService';
import vehicleService from '../services/vehicleService';
import farmService from '../services/farmService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import SuccessAlert from '../components/SuccessAlert';
import './Expenses.css';

const Expenses: React.FC = () => {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [vehicles, setVehicles] = useState<any[]>([]);
  const [farms, setFarms] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingExpense, setEditingExpense] = useState<Expense | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState({
    category: '',
    startDate: '',
    endDate: '',
  });
  const [formData, setFormData] = useState<CreateExpenseDto>({
    expenseType: 'Fuel',
    category: '',
    amount: 0,
    description: '',
    expenseDate: new Date().toISOString().split('T')[0],
  });

  useEffect(() => {
    loadExpenses();
    loadVehicles();
    loadFarms();
  }, [pageNumber, filters]);

  const loadExpenses = async () => {
    try {
      setLoading(true);
      setError('');
      let response;
      if (filters.category) {
        response = await expenseService.getByCategory(filters.category, pageNumber, 10);
      } else if (filters.startDate && filters.endDate) {
        response = await expenseService.getByDateRange(filters.startDate, filters.endDate, pageNumber, 10);
      } else {
        response = await expenseService.getAll(pageNumber, 10);
      }
      if (response.success && response.data) {
        setExpenses(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load expenses');
    } finally {
      setLoading(false);
    }
  };

  const loadVehicles = async () => {
    try {
      const response = await vehicleService.getAll(1, 100);
      if (response.success && response.data) {
        setVehicles(response.data.items);
      }
    } catch (err) {
      console.error('Failed to load vehicles:', err);
    }
  };

  const loadFarms = async () => {
    try {
      const response = await farmService.getAll(1, 100);
      if (response.success && response.data) {
        setFarms(response.data.items);
      }
    } catch (err) {
      console.error('Failed to load farms:', err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setError('');
      if (editingExpense) {
        await expenseService.update(editingExpense.id, formData);
        setSuccess('Expense updated successfully');
      } else {
        await expenseService.create(formData);
        setSuccess('Expense created successfully');
      }
      setTimeout(() => setSuccess(''), 3000);
      setShowModal(false);
      resetForm();
      loadExpenses();
    } catch (err: any) {
      setError(err.message || 'Failed to save expense');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this expense?')) return;

    try {
      setError('');
      await expenseService.delete(id);
      setSuccess('Expense deleted successfully');
      setTimeout(() => setSuccess(''), 3000);
      loadExpenses();
    } catch (err: any) {
      setError(err.message || 'Failed to delete expense');
    }
  };

  const resetForm = () => {
    setFormData({
      expenseType: 'Fuel',
      category: '',
      amount: 0,
      description: '',
      expenseDate: new Date().toISOString().split('T')[0],
    });
    setEditingExpense(null);
  };

  const openEditModal = (expense: Expense) => {
    setEditingExpense(expense);
    setFormData({
      expenseType: expense.expenseType,
      category: expense.category,
      amount: expense.amount,
      description: expense.description,
      expenseDate: expense.expenseDate.split('T')[0],
      vehicleId: expense.vehicleId,
      farmId: expense.farmId,
    });
    setShowModal(true);
  };

  const getExpenseTypeBadgeClass = (type: string) => {
    const typeMap: { [key: string]: string } = {
      'Fuel': 'warning',
      'VehicleMaintenance': 'info',
      'Salary': 'success',
      'Feed': 'warning',
      'Utilities': 'info',
      'Other': 'pending',
    };
    return typeMap[type] || 'pending';
  };

  if (loading && expenses.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="expenses-page">
      <div className="page-header">
        <h1>Expense Tracking</h1>
        <button className="btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          Add Expense
        </button>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}
      {success && <SuccessAlert message={success} onClose={() => setSuccess('')} />}

      <div className="filters">
        <select
          value={filters.category}
          onChange={(e) => setFilters({ ...filters, category: e.target.value })}
        >
          <option value="">All Categories</option>
          <option value="Fuel">Fuel</option>
          <option value="VehicleMaintenance">Vehicle Maintenance</option>
          <option value="Salary">Salary</option>
          <option value="Feed">Feed</option>
          <option value="Utilities">Utilities</option>
          <option value="Other">Other</option>
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
        {(filters.category || filters.startDate || filters.endDate) && (
          <button className="btn-secondary" onClick={() => setFilters({ category: '', startDate: '', endDate: '' })}>
            Clear Filters
          </button>
        )}
      </div>

      <table className="expenses-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Type</th>
            <th>Category</th>
            <th>Amount</th>
            <th>Description</th>
            <th>Vehicle/Farm</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {expenses.map((expense) => (
            <tr key={expense.id}>
              <td>{new Date(expense.expenseDate).toLocaleDateString()}</td>
              <td>
                <span className={`status-badge ${getExpenseTypeBadgeClass(expense.expenseType)}`}>
                  {expense.expenseType}
                </span>
              </td>
              <td>{expense.category}</td>
              <td>${expense.amount.toFixed(2)}</td>
              <td>{expense.description}</td>
              <td>{expense.vehicleNumber || expense.farmName || '-'}</td>
              <td>
                <button className="btn-edit" onClick={() => openEditModal(expense)}>Edit</button>
                <button className="btn-delete" onClick={() => handleDelete(expense.id)}>Delete</button>
              </td>
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
            <h2>{editingExpense ? 'Edit Expense' : 'Add Expense'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Expense Type *</label>
                <select
                  value={formData.expenseType}
                  onChange={(e) => setFormData({ ...formData, expenseType: e.target.value })}
                  required
                >
                  <option value="Fuel">Fuel</option>
                  <option value="VehicleMaintenance">Vehicle Maintenance</option>
                  <option value="Salary">Salary</option>
                  <option value="Feed">Feed</option>
                  <option value="Utilities">Utilities</option>
                  <option value="Other">Other</option>
                </select>
              </div>
              <div className="form-group">
                <label>Category *</label>
                <input
                  type="text"
                  value={formData.category}
                  onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Amount *</label>
                <input
                  type="number"
                  step="0.01"
                  value={formData.amount || ''}
                  onChange={(e) => setFormData({ ...formData, amount: parseFloat(e.target.value) || 0 })}
                  required
                  min="0"
                />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows={3}
                />
              </div>
              <div className="form-group">
                <label>Expense Date *</label>
                <input
                  type="date"
                  value={formData.expenseDate}
                  onChange={(e) => setFormData({ ...formData, expenseDate: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Vehicle (Optional)</label>
                <select
                  value={formData.vehicleId || ''}
                  onChange={(e) => setFormData({ ...formData, vehicleId: e.target.value || undefined })}
                >
                  <option value="">None</option>
                  {vehicles.map((vehicle) => (
                    <option key={vehicle.id} value={vehicle.id}>
                      {vehicle.vehicleNumber}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Farm (Optional)</label>
                <select
                  value={formData.farmId || ''}
                  onChange={(e) => setFormData({ ...formData, farmId: e.target.value || undefined })}
                >
                  <option value="">None</option>
                  {farms.map((farm) => (
                    <option key={farm.id} value={farm.id}>
                      {farm.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="modal-actions">
                <button type="submit" className="btn-primary">
                  {editingExpense ? 'Update' : 'Create'}
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

export default Expenses;
