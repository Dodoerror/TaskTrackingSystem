using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTrackingSystem.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuAdminDetails",
                columns: table => new
                {
                    MenuAdminDetailId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MenuDetailCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentMenuCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApiName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    DelFlag = table.Column<int>(type: "int", nullable: false),
                    CreatedUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAdminDetails", x => x.MenuAdminDetailId);
                });

            migrationBuilder.CreateTable(
                name: "MenuAdmins",
                columns: table => new
                {
                    AdminMenuId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MenuCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MenuUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DelFlag = table.Column<int>(type: "int", nullable: false),
                    CreatedUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAdmins", x => x.AdminMenuId);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenus",
                columns: table => new
                {
                    RoleMenuId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MenuCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DelFlag = table.Column<int>(type: "int", nullable: false),
                    CreatedUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenus", x => x.RoleMenuId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuAdminDetails");

            migrationBuilder.DropTable(
                name: "MenuAdmins");

            migrationBuilder.DropTable(
                name: "RoleMenus");
        }
    }
}
