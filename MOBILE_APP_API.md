# Mobile Driver App API Documentation

## Overview
This document outlines the API endpoints required for the mobile driver application.

## Authentication
- **POST** `/api/auth/login` - Driver login
- **POST** `/api/auth/refresh` - Refresh token

## Driver Endpoints

### Get Assigned Deliveries
- **GET** `/api/deliveries/my-assigned`
  - Returns deliveries assigned to the logged-in driver
  - Query params: `status` (optional), `pageNumber`, `pageSize`
  - Response: `PagedResult<DeliveryDto>`

### Get Delivery Details
- **GET** `/api/deliveries/{id}`
  - Returns detailed delivery information
  - Response: `DeliveryDto`

### Update Delivery Status
- **PUT** `/api/deliveries/{id}/status`
  - Body: `{ "status": "InTransit" | "Completed", "verifiedQuantity": number }`
  - Updates delivery status and verified quantity

### GPS Tracking
- **POST** `/api/deliveries/{id}/location`
  - Body: `{ "latitude": number, "longitude": number, "timestamp": DateTime }`
  - Records driver's current location for a delivery

### Upload Delivery Photo
- **POST** `/api/deliveries/{id}/photo`
  - Content-Type: `multipart/form-data`
  - Body: `file` (image)
  - Uploads delivery confirmation photo

## Implementation Notes

### Mobile App Structure (React Native Example)
```
mobile-driver-app/
├── src/
│   ├── screens/
│   │   ├── LoginScreen.tsx
│   │   ├── DeliveriesScreen.tsx
│   │   ├── DeliveryDetailScreen.tsx
│   │   └── ProfileScreen.tsx
│   ├── services/
│   │   ├── api.ts
│   │   ├── deliveryService.ts
│   │   └── locationService.ts
│   ├── components/
│   │   ├── DeliveryCard.tsx
│   │   └── MapView.tsx
│   └── navigation/
│       └── AppNavigator.tsx
├── package.json
└── README.md
```

### Key Features
1. **Real-time Location Tracking**: Use React Native Geolocation API
2. **Offline Support**: Cache deliveries locally using AsyncStorage
3. **Push Notifications**: Receive delivery assignments via FCM/APNS
4. **Photo Capture**: Use react-native-image-picker for delivery photos
5. **Map Integration**: Use react-native-maps for route visualization

### Required Backend Updates
- Add `DriverId` to `Delivery` entity (if not already present)
- Add `Location` entity for GPS tracking
- Add `DeliveryPhoto` entity for photo storage
- Create driver-specific endpoints in `DeliveriesController`
