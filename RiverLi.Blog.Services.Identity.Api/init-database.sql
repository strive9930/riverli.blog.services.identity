-- =============================================
-- RiverLi.Blog.Identity 数据库初始化脚本
-- 数据库：MySQL 8.0+
-- 字符集：utf8mb4
-- 排序规则：utf8mb4_unicode_ci
-- =============================================

-- 创建数据库
CREATE DATABASE IF NOT EXISTS `RiverBlog_Identity` 
DEFAULT CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `RiverBlog_Identity`;

-- =============================================
-- 1. Identity 核心表
-- =============================================

-- Users 表 (AspNetUsers)
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL,
    `UserName` VARCHAR(256),
    `NormalizedUserName` VARCHAR(256),
    `Email` VARCHAR(256),
    `NormalizedEmail` VARCHAR(256),
    `EmailConfirmed` TINYINT(1) NOT NULL DEFAULT 0,
    `PasswordHash` LONGTEXT,
    `SecurityStamp` LONGTEXT,
    `ConcurrencyStamp` LONGTEXT,
    `PhoneNumber` LONGTEXT,
    `PhoneNumberConfirmed` TINYINT(1) NOT NULL DEFAULT 0,
    `TwoFactorEnabled` TINYINT(1) NOT NULL DEFAULT 0,
    `LockoutEnd` DATETIME(6),
    `LockoutEnabled` TINYINT(1) NOT NULL DEFAULT 0,
    `AccessFailedCount` INT NOT NULL DEFAULT 0,
    `NickName` VARCHAR(100),
    `AvatarUrl` VARCHAR(500),
    `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    INDEX `IX_Users_Email` (`Email`),
    INDEX `IX_Users_NormalizedEmail` (`NormalizedEmail`),
    INDEX `IX_Users_NormalizedUserName` (`NormalizedUserName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Roles 表 (AspNetRoles)
CREATE TABLE IF NOT EXISTS `Roles` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(256),
    `NormalizedName` VARCHAR(256),
    `ConcurrencyStamp` LONGTEXT,
    `Code` VARCHAR(50),
    `Description` VARCHAR(500),
    `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `ParentRoleId` CHAR(36),
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Roles_Code` (`Code`),
    INDEX `IX_Roles_NormalizedName` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserClaims 表
CREATE TABLE IF NOT EXISTS `UserClaims` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` CHAR(36) NOT NULL,
    `ClaimType` LONGTEXT,
    `ClaimValue` LONGTEXT,
    PRIMARY KEY (`Id`),
    INDEX `IX_UserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_UserClaims_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserLogins 表
CREATE TABLE IF NOT EXISTS `UserLogins` (
    `LoginProvider` VARCHAR(128) NOT NULL,
    `ProviderKey` VARCHAR(128) NOT NULL,
    `ProviderDisplayName` LONGTEXT,
    `UserId` CHAR(36) NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    INDEX `IX_UserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_UserLogins_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- RoleClaims 表
CREATE TABLE IF NOT EXISTS `RoleClaims` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `RoleId` CHAR(36) NOT NULL,
    `ClaimType` LONGTEXT,
    `ClaimValue` LONGTEXT,
    PRIMARY KEY (`Id`),
    INDEX `IX_RoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_RoleClaims_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserTokens 表
CREATE TABLE IF NOT EXISTS `UserTokens` (
    `UserId` CHAR(36) NOT NULL,
    `LoginProvider` VARCHAR(128) NOT NULL,
    `Name` VARCHAR(128) NOT NULL,
    `Value` LONGTEXT,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_UserTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserRoles 表 (用户 - 角色关联)
CREATE TABLE IF NOT EXISTS `UserRoles` (
    `UserId` CHAR(36) NOT NULL,
    `RoleId` CHAR(36) NOT NULL,
    PRIMARY KEY (`UserId`, `RoleId`),
    INDEX `IX_UserRoles_RoleId` (`RoleId`),
    CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- 2. 权限管理表
-- =============================================

-- Permissions 表
CREATE TABLE IF NOT EXISTS `Permissions` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Code` VARCHAR(50) NOT NULL,
    `Description` VARCHAR(500),
    `Group` VARCHAR(100) NOT NULL,
    `ClaimType` VARCHAR(200),
    `ClaimValue` VARCHAR(200),
    `RoleId` CHAR(36),
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Permissions_Code` (`Code`),
    INDEX `IX_Permissions_Group` (`Group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- RolePermissions 表 (角色 - 权限关联)
CREATE TABLE IF NOT EXISTS `RolePermissions` (
    `RoleId` CHAR(36) NOT NULL,
    `PermissionId` CHAR(36) NOT NULL,
    PRIMARY KEY (`RoleId`, `PermissionId`),
    INDEX `IX_RolePermissions_PermissionId` (`PermissionId`),
    CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RolePermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserPermissions 表 (用户 - 权限关联)
CREATE TABLE IF NOT EXISTS `UserPermissions` (
    `UserId` CHAR(36) NOT NULL,
    `PermissionId` CHAR(36) NOT NULL,
    PRIMARY KEY (`UserId`, `PermissionId`),
    INDEX `IX_UserPermissions_PermissionId` (`PermissionId`),
    CONSTRAINT `FK_UserPermissions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserPermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- 3. 路由管理表
-- =============================================

-- RouteGroups 表
CREATE TABLE IF NOT EXISTS `RouteGroups` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Code` VARCHAR(50) NOT NULL,
    `Description` VARCHAR(500),
    `Icon` VARCHAR(100),
    `GroupType` INT NOT NULL,
    `Sort` INT NOT NULL DEFAULT 0,
    `RequiredPermission` VARCHAR(100),
    `ParentId` CHAR(36),
    `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_RouteGroups_Code` (`Code`),
    INDEX `IX_RouteGroups_ParentId` (`ParentId`),
    CONSTRAINT `FK_RouteGroups_RouteGroups_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `RouteGroups` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- SysRoutes 表 (后端路由)
CREATE TABLE IF NOT EXISTS `SysRoutes` (
    `Id` CHAR(36) NOT NULL,
    `Path` VARCHAR(200) NOT NULL,
    `Method` VARCHAR(20) NOT NULL,
    `RequiredPermission` VARCHAR(200),
    `RouteGroupId` CHAR(36),
    `FrontendRouteId` CHAR(36),
    `ServiceName` VARCHAR(100),
    `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_SysRoutes_Path_Method` (`Path`, `Method`),
    INDEX `IX_SysRoutes_RouteGroupId` (`RouteGroupId`),
    CONSTRAINT `FK_SysRoutes_RouteGroups_RouteGroupId` FOREIGN KEY (`RouteGroupId`) REFERENCES `RouteGroups` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- FrontendRoutes 表 (前端路由)
CREATE TABLE IF NOT EXISTS `FrontendRoutes` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Path` VARCHAR(200) NOT NULL,
    `Component` VARCHAR(200) NOT NULL,
    `Title` VARCHAR(100) NOT NULL,
    `Icon` VARCHAR(100),
    `Sort` INT NOT NULL DEFAULT 0,
    `Hidden` TINYINT(1) NOT NULL DEFAULT 0,
    `Redirect` VARCHAR(200),
    `AlwaysShow` TINYINT(1) NOT NULL DEFAULT 0,
    `KeepAlive` TINYINT(1) NOT NULL DEFAULT 0,
    `RequiredPermission` VARCHAR(100),
    `ParentId` CHAR(36),
    `RouteGroupId` CHAR(36),
    `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_FrontendRoutes_Path` (`Path`),
    UNIQUE INDEX `IX_FrontendRoutes_Name` (`Name`),
    INDEX `IX_FrontendRoutes_ParentId` (`ParentId`),
    INDEX `IX_FrontendRoutes_RouteGroupId` (`RouteGroupId`),
    CONSTRAINT `FK_FrontendRoutes_FrontendRoutes_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `FrontendRoutes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_FrontendRoutes_RouteGroups_RouteGroupId` FOREIGN KEY (`RouteGroupId`) REFERENCES `RouteGroups` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- FrontendRouteSysRoutes 表 (前后端路由关联)
CREATE TABLE IF NOT EXISTS `FrontendRouteSysRoutes` (
    `FrontendRouteId` CHAR(36) NOT NULL,
    `SysRouteId` CHAR(36) NOT NULL,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`FrontendRouteId`, `SysRouteId`),
    INDEX `IX_FrontendRouteSysRoutes_SysRouteId` (`SysRouteId`),
    CONSTRAINT `FK_FrontendRouteSysRoutes_FrontendRoutes_FrontendRouteId` FOREIGN KEY (`FrontendRouteId`) REFERENCES `FrontendRoutes` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_FrontendRouteSysRoutes_SysRoutes_SysRouteId` FOREIGN KEY (`SysRouteId`) REFERENCES `SysRoutes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- 4. 菜单管理表
-- =============================================

-- MenuGroups 表
CREATE TABLE IF NOT EXISTS `MenuGroups` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Code` VARCHAR(50) NOT NULL,
    `Description` VARCHAR(500),
    `Icon` VARCHAR(100),
    `Sort` INT NOT NULL DEFAULT 0,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_MenuGroups_Code` (`Code`),
    UNIQUE INDEX `IX_MenuGroups_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Menus 表
CREATE TABLE IF NOT EXISTS `Menus` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Title` VARCHAR(100) NOT NULL,
    `Path` VARCHAR(200),
    `Icon` VARCHAR(100),
    `Sort` INT NOT NULL DEFAULT 0,
    `MenuType` INT NOT NULL,
    `Description` VARCHAR(500),
    `RequiredPermission` VARCHAR(100),
    `ParentId` CHAR(36),
    `MenuGroupId` CHAR(36),
    `FrontendRouteId` CHAR(36),
    `IsVisible` TINYINT(1) NOT NULL DEFAULT 1,
    `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Menus_Name` (`Name`),
    INDEX `IX_Menus_ParentId` (`ParentId`),
    INDEX `IX_Menus_MenuGroupId` (`MenuGroupId`),
    INDEX `IX_Menus_FrontendRouteId` (`FrontendRouteId`),
    CONSTRAINT `FK_Menus_Menus_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `Menus` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Menus_MenuGroups_MenuGroupId` FOREIGN KEY (`MenuGroupId`) REFERENCES `MenuGroups` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Menus_FrontendRoutes_FrontendRouteId` FOREIGN KEY (`FrontendRouteId`) REFERENCES `FrontendRoutes` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- 5. 用户登录历史表
-- =============================================

-- UserLoginHistories 表
CREATE TABLE IF NOT EXISTS `UserLoginHistories` (
    `Id` CHAR(36) NOT NULL,
    `UserId` CHAR(36) NOT NULL,
    `IpAddress` VARCHAR(45) NOT NULL,
    `Location` VARCHAR(100),
    `DeviceInfo` VARCHAR(200) NOT NULL,
    `BrowserInfo` VARCHAR(100),
    `OperatingSystem` VARCHAR(100),
    `Status` VARCHAR(20) NOT NULL,
    `FailureReason` VARCHAR(500),
    `UserAgent` VARCHAR(500),
    `SessionId` VARCHAR(100),
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    INDEX `IX_UserLoginHistories_UserId` (`UserId`),
    INDEX `IX_UserLoginHistories_CreateTime` (`CreateTime`),
    INDEX `IX_UserLoginHistories_IpAddress` (`IpAddress`),
    INDEX `IX_UserLoginHistories_Status` (`Status`),
    CONSTRAINT `FK_UserLoginHistories_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- 6. 初始数据
-- =============================================

-- 插入默认角色
INSERT INTO `Roles` (`Id`, `Name`, `NormalizedName`, `Code`, `Description`, `IsEnabled`, `CreateTime`) VALUES
(UUID(), 'Admin', 'ADMIN', 'admin', 'Administrator role', 1, NOW()),
(UUID(), 'User', 'USER', 'user', 'Normal user role', 1, NOW());

-- 插入默认管理员账号 (密码：Admin123!)
-- 注意：这里的 PasswordHash 是示例，实际应该使用 Identity 的密码哈希器生成
INSERT INTO `Users` (`Id`, `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `EmailConfirmed`, `PasswordHash`, 
    `SecurityStamp`, `NickName`, `IsEnabled`, `CreateTime`) VALUES
(UUID(), 'admin@example.com', 'ADMIN@EXAMPLE.COM', 'admin@example.com', 'ADMIN@EXAMPLE.COM', 1, 
    'AQAAAAIAAYagAAAAEH7+9GqjNlQwK3MxYzQwZjE2LTg5ZjYtNGU2OS05MTI5LTY5MzQ5ZjE2OTY5OQ==',
    UUID(), '超级管理员', 1, NOW());

-- 将管理员分配到 Admin 角色
INSERT INTO `UserRoles` (`UserId`, `RoleId`)
SELECT u.Id, r.Id FROM `Users` u, `Roles` r WHERE u.Email = 'admin@example.com' AND r.Name = 'Admin';

-- =============================================
-- 说明
-- =============================================
-- 1. 所有主键使用 CHAR(36) 存储 GUID/UUID
-- 2. 时间字段使用 DATETIME，自动设置默认值和更新值
-- 3. 布尔值使用 TINYINT(1)
-- 4. 外键约束都设置了级联删除
-- 5. 重要字段都有适当的索引
-- 6. 字符集统一使用 utf8mb4，支持 emoji 等特殊字符
-- 7. 引擎使用 InnoDB，支持事务和外键
