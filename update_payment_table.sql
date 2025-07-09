-- SQL script to update Payment table for deposit functionality
-- Add new columns to Payment table

ALTER TABLE Payments 
ADD COLUMN PaymentType VARCHAR(50) DEFAULT 'Full' COMMENT 'Full, Deposit, Final',
ADD COLUMN DepositPercentage DECIMAL(3,2) NULL COMMENT 'For deposit payments (e.g., 0.20 for 20%)';

-- Update existing records to have PaymentType = 'Full'
UPDATE Payments SET PaymentType = 'Full' WHERE PaymentType IS NULL;

-- Add index for better performance
CREATE INDEX idx_payments_type ON Payments(PaymentType);
CREATE INDEX idx_payments_order_type ON Payments(OrderId, PaymentType);
