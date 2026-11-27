--记录数据库版本 升级表需要
DROP TABLE IF EXISTS DB_VERSION;
CREATE TABLE DB_VERSION
(
    ID INTEGER PRIMARY KEY,
    Version VARCHAR(6)
);
INSERT INTO DB_VERSION(ID,Version) VALUES ( 1, '1.0');

--医院信息
DROP TABLE IF EXISTS HospitalInfo;
CREATE TABLE HospitalInfo(
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Code VARCHAR(32),
    Name VARCHAR(32),
    Dept VARCHAR(32),
    Address VARCHAR(32),
    Desc VARCHAR(32),
    Telephone VARCHAR(16),
    ModifyTime DATE,
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);
INSERT INTO HospitalInfo(ID,Code,Name,Dept,Address,Desc,Telephone) VALUES ( 1, '', '南京康复医院', '康复科', '', '', '');
--管理员
DROP TABLE IF EXISTS AdminInfo;
CREATE TABLE AdminInfo(
    Account VARCHAR(32) PRIMARY KEY NOT NULL,
    Password VARCHAR(32) NOT NULL,
    Role SMALLINT NOT NULL,
    Sex SMALLINT,
    Dept VARCHAR(32),
    Name VARCHAR(32),
    Telephone VARCHAR(16),
    IsForbidden Boolean NOT NULL,
    Creator VARCHAR(32) NOT NULL,
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);

update AdminInfo set Role=1 where Account= "1";
-- SELECT * FROM AdminInfo;
--INSERT INTO AdminInfo(Account,Password,Dept,Telephone,Role) VALUES ( 'admin', '1234', "康复部",'13564984964',1);
--update AdminInfo set CreateTime=datetime('now','localtime') where Account= "admin";
--患者详情
DROP TABLE IF EXISTS PatientInfo;
CREATE TABLE PatientInfo(
    ID VARCHAR(32) PRIMARY KEY NOT NULL,
    Name VARCHAR(16) NOT NULL,
    Sex SMALLINT NOT NULL,
    Age SMALLINT NOT NULL,
    AffectedSide SMALLINT NOT NULL,
    Stature SMALLINT NOT NULL,
    Weight SMALLINT NOT NULL,
    CardNum VARCHAR(32),
    Doctor VARCHAR(16) NOT NULL,
    Diagnose VARCHAR(256),
    IsCalibrated BOOLEAN NOT NULL DEFAULT 0,
    CalibrationTime DATE,
    AdmissionDate DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);
-- SELECT * FROM PatientInfo;
--BirthDate DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
--ModifyTime DATE NOT NULL DEFAULT (datetime('now','localtime')),

--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0001','张三', 1, '罗翔', '',  '12',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0002','卖瓜的小贩', 0,  '祥子', '',  '122',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0003','陈五', 1, '罗翔', '',  '63',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0004','李二', 1,  '罗翔', '',  '34',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0005','王六', 0,  '罗翔', '',  '45',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0006','刘大', 1,  '罗翔', '',  '58',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0008','许仙', 1,  '法海', '',  '96',  '2021/11/1');

--康复评估与训练报告
DROP TABLE IF EXISTS RehabReport;
CREATE TABLE RehabReport(
    ReportID INTEGER PRIMARY KEY AUTOINCREMENT,
    Title VARCHAR(64) NOT NULL,
    PatientID VARCHAR(32) NOT NULL,
    Kind SMALLINT NOT NULL,
    Joint SMALLINT NOT NULL,
    PayloadJson VARCHAR(1024) NOT NULL,
    StartTime DATE NOT NULL,
    EndTime DATE NOT NULL,
    CreateTime DATE NOT NULL
);

CREATE TABLE RehabReportSample(
    SampleID INTEGER PRIMARY KEY AUTOINCREMENT,
    ReportID INTEGER NOT NULL,
    SampleIndex INTEGER NOT NULL,
    TimestampSeconds REAL NOT NULL,
    PositionX REAL NOT NULL,
    PositionY REAL NOT NULL,
    PositionZ REAL NOT NULL,
    JointAngleX REAL NOT NULL,
    JointAngleY REAL NOT NULL,
    JointAngleZ REAL NOT NULL,
    ForceX REAL NOT NULL,
    ForceY REAL NOT NULL,
    ForceZ REAL NOT NULL,
    TorqueX REAL NOT NULL,
    TorqueY REAL NOT NULL,
    TorqueZ REAL NOT NULL,
    CurrentAngleDegrees REAL NOT NULL,
    SpeedMetersPerSecond REAL NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_RehabReportSample_ReportID ON RehabReportSample(ReportID);

--治疗方案与疗程方案共用
DROP TABLE IF EXISTS TreatmentPlan;
CREATE TABLE TreatmentPlan(
    PlanID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(64) NOT NULL,
    Mode SMALLINT NOT NULL,
    Direction VARCHAR(256),
    IsDirectionRestricted BOOLEAN NOT NULL DEFAULT 0,
    DurationMinutes SMALLINT NOT NULL,
    ResistanceLevel SMALLINT NOT NULL DEFAULT 3,
    GameItemsJson VARCHAR(8192),
    CreateTime DATE NOT NULL,
    UpdateTime DATE NOT NULL
);
