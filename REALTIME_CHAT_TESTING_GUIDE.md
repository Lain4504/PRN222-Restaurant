k# 💬 Real-time Chat Feature Testing Guide

This guide will help you test the comprehensive real-time chat feature between customers and staff using SignalR.

## 🚀 Getting Started

### 1. Apply Database Migration
First, you need to create and apply the database migration for the chat tables:

```bash
# Create migration (when database is running)
export PATH="$PATH:/home/thanhphuong/.dotnet/tools"
dotnet ef migrations add AddChatFeature

# Apply migration
dotnet ef database update
```

### 2. Start Your Application
```bash
dotnet run
```

The application should start and be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## 🧪 Testing Scenarios

### Scenario 1: Customer Initiates Chat

#### Step 1: Customer Side
1. **Navigate to Customer Chat**: Go to `/Chat`
2. **Expected Result**: 
   - Clean chat interface loads
   - Connection status shows "Connected"
   - Welcome message appears
   - Input field is ready for typing

#### Step 2: Send First Message
1. **Type a message**: "Hello, I need help with my order"
2. **Press Enter or click Send**
3. **Expected Result**:
   - Message appears in chat with timestamp
   - Message is aligned to the right (customer's message)
   - Character counter updates
   - Input field clears

#### Step 3: Verify Chat Room Creation
1. **Check Database**: A new ChatRoom should be created
2. **Check Status**: Chat room status should be "Open"
3. **No Staff Assigned**: StaffId should be null initially

### Scenario 2: Staff Responds to Customer

#### Step 1: Staff Access
1. **Navigate to Staff Chat**: Go to `/Admin/Chat`
2. **Expected Result**:
   - Chat rooms sidebar shows the new customer chat
   - Chat room shows "Unassigned" status
   - Customer name is displayed
   - Unread message count appears

#### Step 2: Join Chat Room
1. **Click on the chat room** in the sidebar
2. **Expected Result**:
   - Chat area opens on the right
   - Customer's message is visible
   - "Assign to Me" button is available
   - Customer info is displayed in header

#### Step 3: Assign and Respond
1. **Click "Assign to Me"**
2. **Type a response**: "Hello! I'm here to help. What's your order number?"
3. **Send the message**
4. **Expected Result**:
   - Staff is assigned to the chat room
   - System message appears: "Staff Name has joined the conversation"
   - Staff message appears (left-aligned)
   - Customer receives the message in real-time

### Scenario 3: Real-time Features

#### Test Typing Indicators
1. **Customer types** (don't send yet)
2. **Expected Result**: Staff sees "Customer is typing..." indicator
3. **Staff types** (don't send yet)
4. **Expected Result**: Customer sees "Support is typing..." indicator

#### Test Online Status
1. **Open multiple browser tabs/windows**
2. **Expected Result**: 
   - Online indicators show correctly
   - Connection status updates in real-time

#### Test Message Delivery
1. **Send messages from both sides**
2. **Expected Result**:
   - Messages appear instantly on both sides
   - Timestamps are accurate
   - Message order is preserved

### Scenario 4: Multiple Chat Rooms

#### Step 1: Create Multiple Chats
1. **Open incognito/private browser window**
2. **Navigate to `/Chat`** (this simulates a different customer)
3. **Send a message**: "I have a question about the menu"
4. **Expected Result**: New chat room is created

#### Step 2: Staff Management
1. **Go to Staff Chat**: `/Admin/Chat`
2. **Expected Result**:
   - Multiple chat rooms appear in sidebar
   - Each shows different customer
   - Unread counts are accurate
   - Most recent chats appear at top

#### Step 3: Switch Between Chats
1. **Click different chat rooms**
2. **Expected Result**:
   - Messages load correctly for each chat
   - No message mixing between chats
   - Proper chat room selection highlighting

### Scenario 5: Chat Management

#### Test Chat Assignment
1. **Unassigned chat exists**
2. **Staff clicks "Assign to Me"**
3. **Expected Result**:
   - Chat status changes to "InProgress"
   - Staff is assigned
   - System message sent to customer

#### Test Chat Closing
1. **Staff clicks "Close Chat"**
2. **Expected Result**:
   - Chat status changes to "Closed"
   - Chat becomes inactive
   - System message sent: "This conversation has been closed"
   - Chat disappears from active list

### Scenario 6: Error Handling

#### Test Connection Loss
1. **Disconnect internet briefly**
2. **Expected Result**:
   - Connection status shows "Reconnecting..."
   - Modal appears showing connection lost
   - Auto-reconnection attempts

#### Test Invalid Messages
1. **Try sending empty messages**
2. **Try sending very long messages (>1000 chars)**
3. **Expected Result**:
   - Empty messages are blocked
   - Character limit is enforced
   - Appropriate error messages

## 🔍 What to Look For

### ✅ Functionality Checklist

#### Real-time Communication
- [ ] **Instant Message Delivery**: Messages appear immediately on both sides
- [ ] **Typing Indicators**: Shows when someone is typing
- [ ] **Online Status**: Accurate online/offline indicators
- [ ] **Auto-reconnection**: Handles connection drops gracefully

#### Chat Management
- [ ] **Chat Room Creation**: New rooms created automatically for customers
- [ ] **Staff Assignment**: Staff can assign themselves to chats
- [ ] **Multiple Chats**: Staff can handle multiple conversations
- [ ] **Chat Closing**: Ability to close completed conversations

#### User Experience
- [ ] **Responsive Design**: Works on desktop and mobile
- [ ] **Message Formatting**: Proper alignment and styling
- [ ] **Timestamps**: Accurate time display
- [ ] **Character Limits**: Input validation works

#### Data Persistence
- [ ] **Message Storage**: Messages saved to database
- [ ] **Chat History**: Previous messages load correctly
- [ ] **User Sessions**: Proper user identification

### ✅ UI/UX Checklist

#### Customer Chat Interface
- [ ] **Clean Design**: Professional, welcoming interface
- [ ] **Easy to Use**: Intuitive message input and sending
- [ ] **Status Indicators**: Clear connection and typing status
- [ ] **Mobile Friendly**: Works well on mobile devices

#### Staff Chat Interface
- [ ] **Chat List**: Clear overview of all active chats
- [ ] **Multi-chat**: Easy switching between conversations
- [ ] **Management Tools**: Assign, close, and manage chats
- [ ] **Notifications**: Visual/audio alerts for new messages

## 🐛 Common Issues & Solutions

### Issue: SignalR Connection Fails
**Solution**: 
1. Check if SignalR is properly configured in Program.cs
2. Verify the hub is mapped correctly
3. Check browser console for JavaScript errors

### Issue: Messages Not Appearing
**Solution**:
1. Check database connection
2. Verify user session is working
3. Check SignalR hub methods are being called

### Issue: Chat Rooms Not Loading
**Solution**:
1. Verify database migration was applied
2. Check if sample data exists
3. Verify API endpoints are working

### Issue: Typing Indicators Not Working
**Solution**:
1. Check SignalR group membership
2. Verify typing event handlers
3. Check for JavaScript errors

## 📊 Test Data

The system will automatically create test users when seeded. You can also manually create:

### Test Users
- **Customer**: Any user with Role = "Customer"
- **Staff**: User with Role = "Staff" 
- **Admin**: User with Role = "Admin"

### Test Scenarios Data
- **Multiple customers**: Use different browser sessions
- **Different message types**: Text, system messages
- **Various chat states**: Open, InProgress, Closed

## 🎯 Success Criteria

Your real-time chat feature is working correctly if:

1. ✅ **Real-time messaging works** between customers and staff
2. ✅ **Multiple chat rooms** can be managed simultaneously
3. ✅ **Staff assignment** and chat management functions work
4. ✅ **Typing indicators** and online status work
5. ✅ **Connection handling** is robust (reconnection, error handling)
6. ✅ **UI is responsive** and user-friendly
7. ✅ **Data persistence** works (messages saved, chat history)
8. ✅ **Performance is good** (fast message delivery, smooth UI)

## 🚨 If Something Doesn't Work

1. **Check the browser console** for JavaScript errors
2. **Check the application logs** for server-side errors
3. **Verify database connection** and migration status
4. **Test SignalR connection** independently
5. **Check user session** and authentication

## 🎉 Advanced Testing

Once basic functionality works, try:

- **Load testing**: Multiple users chatting simultaneously
- **Network conditions**: Test on slow/unstable connections
- **Browser compatibility**: Test on different browsers
- **Mobile testing**: Test on mobile devices
- **Accessibility**: Test with screen readers and keyboard navigation

Happy Testing! 🚀
