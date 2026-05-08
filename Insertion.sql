USE PharmacyDB;
GO

-- 1. [USER] 
INSERT INTO [USER] (Username, Password, Email, Phone, IsActive)
VALUES 
    (N'Youssef', N'hash123', N'john.patient@example.com', N'555-0101', 1),  
    (N'Mohamed', N'hash123', N'jane@example.com', N'555-0102', 1),           
    (N'Shamia', N'hash123', N'bob@example.com', N'555-0103', 1),          
    (N'Neveen', N'hash123', N'alice@pharmacy.com', N'555-0201', 1),     
    (N'Musa', N'hash123', N'john.cashier@pharmacy.com', N'555-0202', 1), 
    (N'Omar', N'hash123', N'phil@pharmacy.com', N'555-0203', 1);       

-- 2. ROLE
INSERT INTO ROLE (RoleName, PermissionsLevel, Description)
VALUES
    (N'System Administrator', 99, N'Full access'),
    (N'Cashier', 10, N'Process transactions'),
    (N'Pharmacist', 50, N'Manage medicines');

-- 3. USER_ROLE
INSERT INTO USER_ROLE (UserID, RoleID)
VALUES
    (4, 1),
    (5, 2),
    (6, 3);

-- 4. PATIENT 
INSERT INTO PATIENT (UserID, Age, Address, PatientBalance)
VALUES
    (1, 35, N'123 Elm St', 0.0),
    (2, 28, N'456 Oak St', 50.50),
    (3, 62, N'789 Pine St', 0.0);

-- 5. PATIENT_ALLERGY
INSERT INTO PATIENT_ALLERGY (UserID, AllergyName)
VALUES
    (1, N'Penicillin'),
    (1, N'Peanuts'),
    (2, N'Dust Mites');

-- 6. EMPLOYEE 
INSERT INTO EMPLOYEE (UserID, Salary, JobType)
VALUES
    (4, 80000, N'Admin'),
    (5, 40000, N'Cashier'),
    (6, 95000, N'Pharmacist');

-- 7. MEDICINE
INSERT INTO MEDICINE (Name, Price, Category, ExpiryDate, StockQuantity, UsageInstructions, IsRefundable)
VALUES
    (N'Paracetamol 500mg', 5.00, N'Painkiller', N'2026-12-31', 500, N'Take 1 pill every 8 hours', 1),
    (N'Amoxicillin 250mg', 15.50, N'Antibiotic', N'2025-06-30', 200, N'Take 1 pill every 12 hours', 0),
    (N'Ibuprofen 400mg', 8.00, N'NSAID', N'2027-01-15', 300, N'Take with food', 1);

-- 8. MED_SIDE_EFFECT
INSERT INTO MED_SIDE_EFFECT (MedicineID, SideEffectName)
VALUES
    (1, N'Nausea'),
    (2, N'Diarrhea'),
    (3, N'Stomach Ulcer');

-- 9. MED_HEALING_EFFECT
INSERT INTO MED_HEALING_EFFECT (MedicineID, HealingEffectName)
VALUES
    (1, N'Fever Reduction'),
    (2, N'Bacterial Infection Clearance'),
    (3, N'Inflammation Reduction');

-- 10. [ORDER] 
INSERT INTO [ORDER] (OrderDate, TotalPrice, Status, PatientID, CashierID)
VALUES
    (N'2023-10-25 10:00', 10.00, N'Completed', 1, 5),
    (N'2023-10-26 11:30', 15.50, N'Pending', 2, 5),
    (N'2023-10-27 14:15', 24.00, N'Completed', 3, 5);

-- 11. ORDER_ITEM
INSERT INTO ORDER_ITEM (OrderID, MedicineID, Quantity, UnitPrice)
VALUES
    (1, 1, 2, 5.00),
    (2, 2, 1, 15.50),
    (3, 3, 3, 8.00);

-- 12. PRESCRIPTION 
INSERT INTO PRESCRIPTION (IssueDate, Status, PatientID, PharmacistID)
VALUES
    (N'2023-10-20', N'Filled', 1, 6),
    (N'2023-10-25', N'Active', 2, 6),
    (N'2023-01-10', N'Expired', 3, 6);

-- 13. PRESCRIPTION_ITEM
INSERT INTO PRESCRIPTION_ITEM (PrescriptionID, MedicineID, PrescribedQuantity)
VALUES
    (1, 2, 14),
    (2, 1, 30),
    (3, 3, 20);

-- 14. PAYMENT 
INSERT INTO PAYMENT (Amount, PaymentDate, PaymentMethod, Status, OrderID)
VALUES
    (10.00, N'2023-10-25 10:05', N'Cash', N'Completed', 1),
    (15.50, N'2023-10-26 11:35', N'Credit Card', N'Pending', 2),
    (24.00, N'2023-10-27 14:16', N'Insurance', N'Completed', 3);
GO