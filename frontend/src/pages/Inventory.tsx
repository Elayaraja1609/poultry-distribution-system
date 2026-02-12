import React, { useState, useEffect } from 'react';
import inventoryService, { FarmInventory } from '../services/inventoryService';
import farmService from '../services/farmService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Inventory.css';

const Inventory: React.FC = () => {
  const [farms, setFarms] = useState<any[]>([]);
  const [selectedFarmId, setSelectedFarmId] = useState<string>('');
  const [inventory, setInventory] = useState<FarmInventory | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadFarms();
  }, []);

  useEffect(() => {
    if (selectedFarmId) {
      loadInventory();
    }
  }, [selectedFarmId]);

  const loadFarms = async () => {
    try {
      setError('');
      const response = await farmService.getAll(1, 100);
      if (response.success && response.data) {
        setFarms(response.data.items);
        if (response.data.items.length > 0 && !selectedFarmId) {
          setSelectedFarmId(response.data.items[0].id);
        }
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load farms');
    }
  };

  const loadInventory = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await inventoryService.getFarmInventory(selectedFarmId);
      if (response.success && response.data) {
        setInventory(response.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load inventory');
    } finally {
      setLoading(false);
    }
  };

  const getStockPercentage = () => {
    if (!inventory || inventory.capacity === 0) return 0;
    return (inventory.currentStock / inventory.capacity) * 100;
  };

  const getStockStatus = () => {
    const percentage = getStockPercentage();
    if (percentage >= 90) return 'critical';
    if (percentage >= 75) return 'warning';
    return 'normal';
  };

  if (loading && !inventory) {
    return <LoadingSpinner />;
  }

  return (
    <div className="inventory-page">
      <div className="page-header">
        <h1>Inventory Tracking</h1>
        <select
          value={selectedFarmId}
          onChange={(e) => setSelectedFarmId(e.target.value)}
          className="farm-selector"
        >
          <option value="">Select Farm</option>
          {farms.map((farm) => (
            <option key={farm.id} value={farm.id}>
              {farm.name}
            </option>
          ))}
        </select>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      {inventory && (
        <div className="inventory-dashboard">
          <div className="inventory-summary">
            <div className="summary-card">
              <h3>Farm: {inventory.farmName}</h3>
              <div className="stock-meter">
                <div className="stock-info">
                  <span>Current Stock: {inventory.currentStock}</span>
                  <span>Capacity: {inventory.capacity}</span>
                </div>
                <div className={`stock-bar stock-bar-${getStockStatus()}`}>
                  <div
                    className="stock-fill"
                    style={{ width: `${getStockPercentage()}%` }}
                  />
                </div>
                <span className="stock-percentage">{getStockPercentage().toFixed(1)}%</span>
              </div>
            </div>

            <div className="summary-stats">
              <div className="stat-card stat-in">
                <h4>Stock In</h4>
                <p>{inventory.stockIn}</p>
              </div>
              <div className="stat-card stat-out">
                <h4>Stock Out</h4>
                <p>{inventory.stockOut}</p>
              </div>
              <div className="stat-card stat-loss">
                <h4>Stock Loss</h4>
                <p>{inventory.stockLoss}</p>
              </div>
              <div className="stat-card stat-available">
                <h4>Available</h4>
                <p>{inventory.availableStock}</p>
              </div>
            </div>
          </div>

          <div className="chicken-stocks">
            <h2>Chicken Batches</h2>
            <table className="stocks-table">
              <thead>
                <tr>
                  <th>Batch Number</th>
                  <th>Quantity</th>
                  <th>Available</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {inventory.chickenStocks.map((stock) => (
                  <tr key={stock.chickenId}>
                    <td>{stock.batchNumber}</td>
                    <td>{stock.quantity}</td>
                    <td>{stock.availableQuantity}</td>
                    <td>
                      <span className={`status-badge status-${stock.status.toLowerCase()}`}>
                        {stock.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default Inventory;
