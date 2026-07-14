using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace riverli.blog.services.identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260609083319_InitialRBAC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sys_role_claims_sys_roles_RoleId",
                table: "sys_role_claims");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_user_claims_sys_users_UserId",
                table: "sys_user_claims");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_user_logins_sys_users_UserId",
                table: "sys_user_logins");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_user_roles_sys_roles_RoleId",
                table: "sys_user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_user_roles_sys_users_UserId",
                table: "sys_user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_user_tokens_sys_users_UserId",
                table: "sys_user_tokens");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "sys_users");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "sys_users");

            migrationBuilder.DropIndex(
                name: "IX_sys_user_roles_RoleId",
                table: "sys_user_roles");

            migrationBuilder.DropIndex(
                name: "IX_sys_user_logins_UserId",
                table: "sys_user_logins");

            migrationBuilder.DropIndex(
                name: "IX_sys_user_claims_UserId",
                table: "sys_user_claims");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "sys_roles");

            migrationBuilder.DropIndex(
                name: "IX_sys_role_claims_RoleId",
                table: "sys_role_claims");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "sys_users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "sys_users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "sys_users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "sys_users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "sys_roles",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "sys_roles",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "sys_menus",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "sys_menus");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "sys_users",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "sys_users",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "sys_users",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "sys_users",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "sys_roles",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "sys_roles",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "sys_users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "sys_users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_roles_RoleId",
                table: "sys_user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_logins_UserId",
                table: "sys_user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_claims_UserId",
                table: "sys_user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "sys_roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_role_claims_RoleId",
                table: "sys_role_claims",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_sys_role_claims_sys_roles_RoleId",
                table: "sys_role_claims",
                column: "RoleId",
                principalTable: "sys_roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sys_user_claims_sys_users_UserId",
                table: "sys_user_claims",
                column: "UserId",
                principalTable: "sys_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sys_user_logins_sys_users_UserId",
                table: "sys_user_logins",
                column: "UserId",
                principalTable: "sys_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sys_user_roles_sys_roles_RoleId",
                table: "sys_user_roles",
                column: "RoleId",
                principalTable: "sys_roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sys_user_roles_sys_users_UserId",
                table: "sys_user_roles",
                column: "UserId",
                principalTable: "sys_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sys_user_tokens_sys_users_UserId",
                table: "sys_user_tokens",
                column: "UserId",
                principalTable: "sys_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
