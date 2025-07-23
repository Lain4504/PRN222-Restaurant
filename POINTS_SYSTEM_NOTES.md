# Points System - Technical Notes

## Points Configuration

### Current Settings
- **PointValue**: 5,000 VND per point
- **MaxPointsUsagePercentage**: 30% (0.3)
- **PointsPerVND**: 0.000005 (1 point per 200,000 VND spent)
- **MinimumOrderAmount**: 100,000 VND
- **MinimumRedemptionPoints**: 1 point
- **WelcomeBonusPoints**: 5 points (25,000 VND value)

## Points Usage Limitation

### Why Users Can't Use All Their Points

**Problem**: User has 93 points but can only use 73 points in checkout.

**Explanation**: 
The system limits points usage to prevent abuse and ensure customers still pay a significant portion of their order.

### Calculation Logic
```csharp
public async Task<int> GetMaxUsablePointsAsync(int userId, decimal orderAmount)
{
    var userPoints = await GetUserPointsAsync(userId);
    var maxDiscountAmount = orderAmount * _config.MaxPointsUsagePercentage;
    var maxPointsFromDiscount = (int)Math.Floor(maxDiscountAmount / _config.PointValue);
    
    return Math.Min(userPoints, Math.Max(0, maxPointsFromDiscount));
}
```

### Example Calculation
For an order worth **1,200,000 VND**:
1. **MaxDiscountAmount** = 1,200,000 × 0.3 = **360,000 VND** (30% of order)
2. **MaxPointsFromDiscount** = 360,000 ÷ 5,000 = **72 points**
3. **MaxUsablePoints** = Min(93, 72) = **72 points**

### Key Points
- ✅ **30% Maximum Discount**: Users can only get up to 30% discount using points
- ✅ **Protection Mechanism**: Ensures customers pay at least 70% of order value
- ✅ **Fair Usage**: Prevents points hoarding and abuse

## Points Flow

### Earning Points
1. **Order Completion**: Points awarded after staff confirms full payment
2. **Calculation**: 1 point per 200,000 VND spent
3. **Welcome Bonus**: 5 points for new customers (25,000 VND value)

### Using Points
1. **Selection**: Users select points in checkout (limited by 30% rule)
2. **Storage**: Points stored in `Order.PointsUsed` field
3. **Redemption**: Points deducted only after successful payment
4. **Rollback**: Points restored if payment fails

## Database Schema

### Order Table
- `PointsUsed`: Number of points used for this order (nullable)
- `TotalPrice`: Final price after points discount

### PointTransaction Table
- `Points`: Positive for earning, negative for spending
- `Type`: "Earned", "Redeemed", "Bonus"
- `RelatedOrderId`: Links to order
- `ExpiresAt`: Point expiration (currently disabled)

## Configuration Changes

To modify points usage limits:
```json
{
  "Points": {
    "MaxPointsUsagePercentage": 0.5  // Change to 50% if needed
  }
}
```

## Recent Fixes

### Points Not Being Deducted Issue
**Problem**: Points selected in checkout weren't being deducted from user account.

**Root Cause**: 
1. Form had conflicting inputs (range slider + hidden input)
2. JavaScript wasn't updating hidden input correctly
3. Points were being redeemed before payment confirmation

**Solution**:
1. ✅ Fixed input conflicts in checkout form
2. ✅ Improved JavaScript to update hidden input properly
3. ✅ Added `PointsUsed` field to Order table
4. ✅ Changed logic to redeem points only after successful payment
5. ✅ Added proper rollback for failed payments

### Current Status
- ✅ Points system working correctly
- ✅ 30% usage limit enforced
- ✅ Points deducted only on successful payment
- ✅ Proper error handling and rollback
