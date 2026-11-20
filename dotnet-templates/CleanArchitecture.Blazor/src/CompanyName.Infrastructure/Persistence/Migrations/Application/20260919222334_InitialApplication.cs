using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyName.Infrastructure.Persistence.Migrations.Application
{
    /// <inheritdoc />
    public partial class InitialApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gl_attachment",
                columns: table => new
                {
                    doc_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    app_doc_id = table.Column<int>(type: "int", nullable: false),
                    doc_name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    doc_type = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    doc_path = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_attachment", x => x.doc_id);
                });

            migrationBuilder.CreateTable(
                name: "gl_parameters",
                columns: table => new
                {
                    gl_parameter_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parameter_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    parameter_code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    parameter_type_id = table.Column<int>(type: "int", nullable: false),
                    parameter_description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    parameter_description_en = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    sort = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_parameters", x => x.gl_parameter_id);
                });

            migrationBuilder.CreateTable(
                name: "gl_parameters_description",
                columns: table => new
                {
                    parameter_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parameter_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    parameter_description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    sort = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_parameters_description", x => x.parameter_type_id);
                });

            migrationBuilder.CreateTable(
                name: "gl_portal_menus",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    menu_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    menu_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    icon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    order_no = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_portal_menus", x => x.menu_id);
                });

            migrationBuilder.CreateTable(
                name: "gl_user_group_menus",
                columns: table => new
                {
                    group_menu_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    group_id = table.Column<int>(type: "int", nullable: true),
                    menu_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_user_group_menus", x => x.group_menu_id);
                });

            migrationBuilder.CreateTable(
                name: "gl_user_groups",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    group_code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_user_groups", x => x.group_id);
                });

            migrationBuilder.CreateTable(
                name: "gl_users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    username = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    fullname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ic_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    role = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<int>(type: "int", nullable: true),
                    aud_add_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_add_by = table.Column<int>(type: "int", nullable: true),
                    aud_mod_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    aud_mod_by = table.Column<int>(type: "int", nullable: true),
                    aud_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aud_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_users", x => x.user_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gl_attachment");

            migrationBuilder.DropTable(
                name: "gl_parameters");

            migrationBuilder.DropTable(
                name: "gl_parameters_description");

            migrationBuilder.DropTable(
                name: "gl_portal_menus");

            migrationBuilder.DropTable(
                name: "gl_user_group_menus");

            migrationBuilder.DropTable(
                name: "gl_user_groups");

            migrationBuilder.DropTable(
                name: "gl_users");
        }
    }
}
