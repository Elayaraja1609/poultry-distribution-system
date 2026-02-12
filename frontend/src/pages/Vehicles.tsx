import React, { useState, useEffect } from 'react';
import vehicleService, { Vehicle, CreateVehicleDto, UpdateVehicleDto } from '../services/vehicleService';
import driverService from '../services/driverService';
import cleanerService from '../services/cleanerService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Vehicles.css';

const Vehicles: React.FC = () => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [drivers, setDrivers] = useState<any[]>([]);
  const [cleaners, setCleaners] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [formData, setFormData] = useState<CreateVehicleDto>({
    vehicleNumber: '',
    model: '',
    capacity: 0,
    driverId: '',
    cleanerId: '',
  });

  useEffect(() => {
    loadData();
  }, [pageNumber]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      
      const [vehiclesResponse, driversResponse, cleanersResponse] = await Promise.all([
        vehicleService.getAll(pageNumber, 10),
        driverService.getAll(1, 100),
        cleanerService.getAll(1, 100),
      ]);

      if (vehiclesResponse.success && vehiclesResponse.data) {
        setVehicles(vehiclesResponse.data.items);
        setTotalPages(vehiclesResponse.data.totalPages);
      }

      if (driversResponse.success && driversResponse.data) {
        setDrivers(driversResponse.data.items);
      }

      if (cleanersResponse.success && cleanersResponse.data) {
        setCleaners(cleanersResponse.data.items);
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
      if (editingVehicle) {
        const updateData: UpdateVehicleDto = {
          ...formData,
          isActive: editingVehicle.isActive,
        };
        await vehicleService.update(editingVehicle.id, updateData);
      } else {
        await vehicleService.create(formData);
      }
      setShowModal(false);
      resetForm();
      loadData();
    } catch (err: any) {
      setError(err.message || 'Failed to save vehicle');
    }
  };

  const handleEdit = (vehicle: Vehicle) => {
    setEditingVehicle(vehicle);
    setFormData({
      vehicleNumber: vehicle.vehicleNumber,
      model: vehicle.model,
      capacity: vehicle.capacity,
      driverId: vehicle.driverId,
      cleanerId: vehicle.cleanerId,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this vehicle?')) {
      try {
        setError('');
        await vehicleService.delete(id);
        loadData();
      } catch (err: any) {
        setError(err.message || 'Failed to delete vehicle');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      vehicleNumber: '',
      model: '',
      capacity: 0,
      driverId: '',
      cleanerId: '',
    });
    setEditingVehicle(null);
  };

  if (loading && vehicles.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="vehicles-page">
      <div className="page-header">
        <h1>Vehicles Management</h1>
        <button className="btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          Add Vehicle
        </button>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <table className="vehicles-table">
        <thead>
          <tr>
            <th>Vehicle Number</th>
            <th>Model</th>
            <th>Capacity</th>
            <th>Driver</th>
            <th>Cleaner</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {vehicles.map((vehicle) => (
            <tr key={vehicle.id}>
              <td>{vehicle.vehicleNumber}</td>
              <td>{vehicle.model}</td>
              <td>{vehicle.capacity}</td>
              <td>{vehicle.driverName}</td>
              <td>{vehicle.cleanerName}</td>
              <td>
                <span className={`status-badge ${vehicle.isActive ? 'active' : 'inactive'}`}>
                  {vehicle.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
              <td>
                <button className="btn-edit" onClick={() => handleEdit(vehicle)}>
                  Edit
                </button>
                <button className="btn-delete" onClick={() => handleDelete(vehicle.id)}>
                  Delete
                </button>
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
            <h2>{editingVehicle ? 'Edit Vehicle' : 'Add Vehicle'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Vehicle Number *</label>
                <input
                  type="text"
                  value={formData.vehicleNumber}
                  onChange={(e) => setFormData({ ...formData, vehicleNumber: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Model *</label>
                <input
                  type="text"
                  value={formData.model}
                  onChange={(e) => setFormData({ ...formData, model: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Capacity *</label>
                <input
                  type="number"
                  value={formData.capacity}
                  onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) || 0 })}
                  required
                  min="1"
                />
              </div>
              <div className="form-group">
                <label>Driver *</label>
                <select
                  value={formData.driverId}
                  onChange={(e) => setFormData({ ...formData, driverId: e.target.value })}
                  required
                >
                  <option value="">Select Driver</option>
                  {drivers.map((driver) => (
                    <option key={driver.id} value={driver.id}>
                      {driver.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Cleaner *</label>
                <select
                  value={formData.cleanerId}
                  onChange={(e) => setFormData({ ...formData, cleanerId: e.target.value })}
                  required
                >
                  <option value="">Select Cleaner</option>
                  {cleaners.map((cleaner) => (
                    <option key={cleaner.id} value={cleaner.id}>
                      {cleaner.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="modal-actions">
                <button type="submit" className="btn-primary">
                  {editingVehicle ? 'Update' : 'Create'}
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

export default Vehicles;
