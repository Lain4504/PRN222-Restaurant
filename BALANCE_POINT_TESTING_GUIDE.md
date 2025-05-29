# BalancePoint Feature Testing Guide

This guide will help you test the BalancePoint feature through the Razor Pages UI.

## 🚀 Getting Started

### 1. Start Your Application
```bash
dotnet run
```

The application should start and be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 2. Navigate to Admin Section
1. Open your browser and go to `https://localhost:5001`
2. Navigate to the admin section (you may need to implement authentication or go directly to admin pages)

## 🧪 Testing Steps

### Step 1: Access BalancePoints Management
1. In the admin sidebar, click on **"Balance Points"**
2. You should see the BalancePoints Index page with a list of all balance points
3. **Expected Result**: A table showing balance points with user information

### Step 2: View Sample Data
The application should automatically seed sample data when running in development mode:
- **John Doe**: $100.50
- **Jane Smith**: $250.75
- **Admin User**: $0.00
- **Bob Johnson**: $75.25
- **Alice Brown**: $500.00

**What to verify:**
- ✅ All 5 balance points are displayed
- ✅ User names and emails are shown correctly
- ✅ Point amounts are formatted as currency
- ✅ User status badges show correctly (Active/Inactive)

### Step 3: Test View Details
1. Click the **👁️ (eye icon)** button for any balance point
2. **Expected Result**: 
   - Detailed view showing balance point information
   - User information displayed in a separate card
   - Action buttons available (Edit, Reset)

### Step 4: Test Edit Functionality
1. From the details page or index page, click the **✏️ (edit icon)** button
2. **Expected Result**: Edit form loads with current values
3. **Test Cases**:
   - Change the point amount to a valid value (e.g., 999.99)
   - Try invalid values:
     - Negative numbers (should show validation error)
     - Values over $999,999.99 (should show validation error)
   - Save valid changes
4. **Expected Result**: 
   - Valid changes save successfully with success message
   - Invalid values show validation errors
   - Redirects to index page after successful save

### Step 5: Test Reset Functionality
1. Choose a balance point with points > 0
2. Click the **↶ (reset icon)** button
3. **Expected Result**: Confirmation modal appears
4. Click "Reset to Zero" in the modal
5. **Expected Result**: 
   - Balance point is reset to $0.00
   - Success message appears
   - Returns to index page

### Step 6: Test Error Handling
1. Try to access a non-existent balance point:
   - Go to `/Admin/BalancePoints/Details/999`
   - **Expected Result**: 404 error or "Not Found" message

## 🔍 What to Look For

### ✅ Functionality Checklist
- [ ] **Data Display**: All balance points load correctly
- [ ] **User Information**: User details show properly linked to balance points
- [ ] **Navigation**: All links work (Index ↔ Details ↔ Edit)
- [ ] **Edit**: Can successfully update balance point amounts
- [ ] **Reset**: Can reset balance points to zero
- [ ] **Validation**: Form validation works for invalid inputs
- [ ] **Messages**: Success/error messages display correctly
- [ ] **Responsive**: Pages work on different screen sizes

### ✅ UI/UX Checklist
- [ ] **Styling**: Pages use consistent styling with your admin theme
- [ ] **Icons**: FontAwesome icons display correctly
- [ ] **Modals**: Confirmation modals work properly
- [ ] **Alerts**: Success/error alerts auto-hide after 5 seconds
- [ ] **Navigation**: Sidebar highlights "Balance Points" when active
- [ ] **Buttons**: All action buttons are clearly labeled and functional

## 🐛 Common Issues & Solutions

### Issue: "Balance Points" link not working
**Solution**: Make sure the route is correct. The pages should be accessible at:
- `/Admin/BalancePoints` (Index)
- `/Admin/BalancePoints/Details/{id}` (Details)
- `/Admin/BalancePoints/Edit/{id}` (Edit)

### Issue: No sample data showing
**Solution**: 
1. Check if the database migration was applied: `dotnet ef database update`
2. Ensure the application is running in Development environment
3. Check the console for any seeding errors

### Issue: Validation not working
**Solution**: Make sure `_ValidationScriptsPartial` is included in the Edit page

### Issue: Styling looks broken
**Solution**: 
1. Ensure CSS files are loading correctly
2. Check if FontAwesome is loading for icons
3. Verify DaisyUI/Tailwind CSS is properly configured

## 📊 Test Data Reference

| User | Email | Initial Points | Status |
|------|-------|---------------|--------|
| John Doe | john.doe@example.com | $100.50 | Active |
| Jane Smith | jane.smith@example.com | $250.75 | Active |
| Admin User | admin@restaurant.com | $0.00 | Active |
| Bob Johnson | bob.johnson@example.com | $75.25 | Active |
| Alice Brown | alice.brown@example.com | $500.00 | Active |

## 🎯 Success Criteria

Your BalancePoint feature is working correctly if:
1. ✅ All pages load without errors
2. ✅ Data displays correctly with proper formatting
3. ✅ CRUD operations work (Create via seeding, Read, Update, Delete via Reset)
4. ✅ Form validation prevents invalid data
5. ✅ User feedback is clear (success/error messages)
6. ✅ Navigation flows smoothly between pages
7. ✅ UI is consistent with your admin theme

## 🚨 If Something Doesn't Work

1. **Check the browser console** for JavaScript errors
2. **Check the application logs** for server-side errors
3. **Verify database connection** and that migrations are applied
4. **Ensure all dependencies** are properly registered in `Program.cs`
5. **Check file paths** and routing configuration

Happy Testing! 🎉
