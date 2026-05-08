USE PharmacyDB;
GO

-- ── Users & Roles ────────────────────────────────────────────────────────────

CREATE TABLE [USER] (
                        UserID        INT           IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(100) NOT NULL,            
    Password      NVARCHAR(255) NOT NULL ,
    Email         NVARCHAR(255) NOT NULL UNIQUE,     
    Phone         NVARCHAR(20),
    IsActive      BIT           NOT NULL DEFAULT 1
    );

CREATE TABLE ROLE (
                      RoleID           INT           IDENTITY(1,1) PRIMARY KEY,
                      RoleName         NVARCHAR(100) NOT NULL,
                      PermissionsLevel INT           NOT NULL,
                      Description      NVARCHAR(500)
);

CREATE TABLE USER_ROLE (
                           UserID INT NOT NULL,
                           RoleID INT NOT NULL,
                           PRIMARY KEY (UserID, RoleID),
                           FOREIGN KEY (UserID) REFERENCES [USER](UserID),
                           FOREIGN KEY (RoleID) REFERENCES ROLE(RoleID)
);

-- ── Patient ──────────────────────────────────────────────────────────────────

CREATE TABLE PATIENT (
                         UserID         INT   PRIMARY KEY,
                         Age            FLOAT,
                         Address        NVARCHAR(500),
                         PatientBalance FLOAT NOT NULL CONSTRAINT DF_Patient_Balance DEFAULT 0.0,
                         FOREIGN KEY (UserID) REFERENCES [USER](UserID)
);

CREATE TABLE PATIENT_ALLERGY (
                                 UserID      INT           NOT NULL,
                                 AllergyName NVARCHAR(100) NOT NULL,
                                 PRIMARY KEY (UserID, AllergyName),
                                 FOREIGN KEY (UserID) REFERENCES PATIENT(UserID)
);

-- ── Employees (Admin / Cashier / Pharmacist all live here) ───────────────────

CREATE TABLE EMPLOYEE (
                          UserID  INT           PRIMARY KEY,
                          Salary  FLOAT         NOT NULL DEFAULT 0,
                          JobType NVARCHAR(50)  NOT NULL CONSTRAINT CHK_Employee_JobType CHECK (JobType IN ('Admin', 'Cashier', 'Pharmacist')),
                          FOREIGN KEY (UserID) REFERENCES [USER](UserID)
);

-- ── Medicine & its multi-value attributes ─────────────────────────────────────

CREATE TABLE MEDICINE (
                          MedicineID        INT            IDENTITY(1,1) PRIMARY KEY,
                          Name              NVARCHAR(200)  NOT NULL,
                          Price             FLOAT          NOT NULL,
                          Category          NVARCHAR(100),
                          ExpiryDate        NVARCHAR(50),
                          StockQuantity     INT            NOT NULL DEFAULT 0,
                          UsageInstructions NVARCHAR(1000),
                          IsRefundable      BIT            NOT NULL DEFAULT 0
);

CREATE TABLE MED_SIDE_EFFECT (
                                 MedicineID     INT           NOT NULL,
                                 SideEffectName NVARCHAR(200) NOT NULL,
                                 PRIMARY KEY (MedicineID, SideEffectName),
                                 FOREIGN KEY (MedicineID) REFERENCES MEDICINE(MedicineID)
);

CREATE TABLE MED_HEALING_EFFECT (
                                    MedicineID        INT           NOT NULL,
                                    HealingEffectName NVARCHAR(200) NOT NULL,
                                    PRIMARY KEY (MedicineID, HealingEffectName),
                                    FOREIGN KEY (MedicineID) REFERENCES MEDICINE(MedicineID)
);

-- ── Orders ───────────────────────────────────────────────────────────────────

CREATE TABLE [ORDER] (
                         OrderID    INT           IDENTITY(1,1) PRIMARY KEY,
    OrderDate  NVARCHAR(50)  NOT NULL,
    TotalPrice FLOAT         NOT NULL DEFAULT 0,
    Status     NVARCHAR(50)  NOT NULL DEFAULT 'Pending' CONSTRAINT CHK_Order_Status CHECK (Status IN ('Pending', 'Completed', 'Cancelled')),
    PatientID  INT           NOT NULL,
    CashierID  INT           NOT NULL,
    FOREIGN KEY (PatientID)  REFERENCES PATIENT(UserID),
    FOREIGN KEY (CashierID)  REFERENCES EMPLOYEE(UserID)
    );

CREATE TABLE ORDER_ITEM (
                            OrderID    INT   NOT NULL,
                            MedicineID INT   NOT NULL,
                            Quantity   INT   NOT NULL,
                            UnitPrice  FLOAT NOT NULL,
                            PRIMARY KEY (OrderID, MedicineID),
                            FOREIGN KEY (OrderID)    REFERENCES [ORDER](OrderID),
                            FOREIGN KEY (MedicineID) REFERENCES MEDICINE(MedicineID)
);

-- ── Prescriptions ────────────────────────────────────────────────────────────

CREATE TABLE PRESCRIPTION (
                              PrescriptionID INT          IDENTITY(1,1) PRIMARY KEY,
                              IssueDate      NVARCHAR(50) NOT NULL,
                              Status         NVARCHAR(50) NOT NULL DEFAULT 'Active' CONSTRAINT CHK_Prescription_Status CHECK (Status IN ('Active', 'Filled', 'Expired', 'Cancelled')),
                              PatientID      INT          NOT NULL,
                              PharmacistID   INT          NOT NULL,
                              FOREIGN KEY (PatientID)    REFERENCES PATIENT(UserID),
                              FOREIGN KEY (PharmacistID) REFERENCES EMPLOYEE(UserID)
);

CREATE TABLE PRESCRIPTION_ITEM (
                                   PrescriptionID     INT NOT NULL,
                                   MedicineID         INT NOT NULL,
                                   PrescribedQuantity INT NOT NULL,
                                   PRIMARY KEY (PrescriptionID, MedicineID),
                                   FOREIGN KEY (PrescriptionID) REFERENCES PRESCRIPTION(PrescriptionID),
                                   FOREIGN KEY (MedicineID)     REFERENCES MEDICINE(MedicineID)
);

-- ── Payments ─────────────────────────────────────────────────────────────────

CREATE TABLE PAYMENT (
                         PaymentID     INT            IDENTITY(1,1) PRIMARY KEY,
                         Amount        FLOAT          NOT NULL,
                         PaymentDate   NVARCHAR(50)   NOT NULL,
                         PaymentMethod NVARCHAR(100)  NOT NULL,
                         Status        NVARCHAR(50)   NOT NULL DEFAULT 'Pending' CONSTRAINT CHK_Payment_Status CHECK (Status IN ('Pending', 'Completed', 'Cancelled')),
                         OrderID       INT            NOT NULL,
                         FOREIGN KEY (OrderID) REFERENCES [ORDER](OrderID)
);
GO