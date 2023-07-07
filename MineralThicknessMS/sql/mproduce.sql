/*
 Navicat Premium Data Transfer

 Source Server         : 本机
 Source Server Type    : MySQL
 Source Server Version : 50724 (5.7.24)
 Source Host           : localhost:3306
 Source Schema         : luojiadata

 Target Server Type    : MySQL
 Target Server Version : 50724 (5.7.24)
 File Encoding         : 65001

 Date: 27/06/2023 11:50:55
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for mproduce
-- ----------------------------
DROP TABLE IF EXISTS `mproduce`;
CREATE TABLE `mproduce`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `date_time` datetime NULL DEFAULT NULL,
  `avg_mine_depth` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `waterway_id` int(11) NULL DEFAULT NULL,
  `rectangle_id` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `datetime`(`date_time`) USING BTREE COMMENT '时间范围查询'
) ENGINE = InnoDB AUTO_INCREMENT = 168 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of mproduce
-- ----------------------------
INSERT INTO `mproduce` VALUES (127, '2023-04-01 20:51:33', '1', 1, 1);
INSERT INTO `mproduce` VALUES (128, '2023-04-01 20:51:33', '1', 2, 1);
INSERT INTO `mproduce` VALUES (129, '2023-04-01 20:51:33', '1', 3, 3);
INSERT INTO `mproduce` VALUES (130, '2023-04-01 20:51:33', '1', 3, 2);
INSERT INTO `mproduce` VALUES (131, '2023-04-01 20:51:33', '1', 4, 4);
INSERT INTO `mproduce` VALUES (132, '2023-05-01 20:51:33', '1', 1, 1);
INSERT INTO `mproduce` VALUES (133, '2023-05-01 20:51:33', '1', 2, 1);
INSERT INTO `mproduce` VALUES (134, '2023-05-01 20:51:33', '1', 3, 3);
INSERT INTO `mproduce` VALUES (135, '2023-05-01 20:51:33', '1', 3, 2);
INSERT INTO `mproduce` VALUES (136, '2023-05-01 20:51:33', '1', 4, 4);
INSERT INTO `mproduce` VALUES (137, '2023-06-20 20:51:33', '1.56', 1, 1);
INSERT INTO `mproduce` VALUES (138, '2023-06-20 20:51:33', '1.4816666666666665', 2, 1);
INSERT INTO `mproduce` VALUES (139, '2023-06-20 20:51:33', '2.628', 3, 3);
INSERT INTO `mproduce` VALUES (140, '2023-06-20 20:51:33', '2.35', 3, 2);
INSERT INTO `mproduce` VALUES (141, '2023-06-20 20:51:33', '1', 4, 4);
INSERT INTO `mproduce` VALUES (142, '2023-06-21 20:51:33', '2.6', 1, 1);
INSERT INTO `mproduce` VALUES (143, '2023-06-21 20:51:33', '1.4816666666666665', 2, 1);
INSERT INTO `mproduce` VALUES (144, '2023-06-21 20:51:33', '3.5', 3, 3);
INSERT INTO `mproduce` VALUES (145, '2023-06-21 20:51:33', '1.3878', 3, 2);
INSERT INTO `mproduce` VALUES (146, '2023-06-21 20:51:33', '2.6', 4, 4);
INSERT INTO `mproduce` VALUES (147, '2023-06-22 20:51:33', '2.6', 1, 1);
INSERT INTO `mproduce` VALUES (148, '2023-06-22 20:51:33', '1.4816666666666665', 2, 1);
INSERT INTO `mproduce` VALUES (149, '2023-06-22 20:51:33', '2.628', 3, 3);
INSERT INTO `mproduce` VALUES (150, '2023-06-22 20:51:33', '2.5', 3, 2);
INSERT INTO `mproduce` VALUES (151, '2023-06-22 20:51:33', '1', 4, 4);
INSERT INTO `mproduce` VALUES (152, '2023-06-23 20:51:33', '2.9', 1, 1);
INSERT INTO `mproduce` VALUES (153, '2023-06-23 20:51:33', '1.4816666666666665', 2, 1);
INSERT INTO `mproduce` VALUES (154, '2023-06-23 20:51:33', '2.5', 3, 3);
INSERT INTO `mproduce` VALUES (155, '2023-06-23 20:51:33', '1.3878', 3, 2);
INSERT INTO `mproduce` VALUES (156, '2023-06-23 20:51:33', '1.9', 4, 4);
INSERT INTO `mproduce` VALUES (157, '2023-06-24 20:51:33', '1.9121666666666663', 1, 1);
INSERT INTO `mproduce` VALUES (158, '2023-06-24 20:51:33', '2.6', 2, 1);
INSERT INTO `mproduce` VALUES (159, '2023-06-24 20:51:33', '2.628', 3, 3);
INSERT INTO `mproduce` VALUES (160, '2023-06-24 20:51:33', '2.5', 3, 2);
INSERT INTO `mproduce` VALUES (161, '2023-06-24 20:51:33', '1', 4, 4);
INSERT INTO `mproduce` VALUES (162, '2023-06-25 20:51:33', '1', 1, 1);
INSERT INTO `mproduce` VALUES (163, '2023-06-25 20:51:33', '1', 2, 1);
INSERT INTO `mproduce` VALUES (164, '2023-06-25 20:51:33', '1', 3, 3);
INSERT INTO `mproduce` VALUES (165, '2023-06-25 08:51:33', '1', 3, 2);
INSERT INTO `mproduce` VALUES (166, '2023-06-26 08:51:33', '1', 4, 4);
INSERT INTO `mproduce` VALUES (167, '2023-06-26 08:51:33', '1', 4, 4);

SET FOREIGN_KEY_CHECKS = 1;
