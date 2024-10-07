﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcWithIdentityAndEFCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClosedAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Closed",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Closed",
                table: "Accounts");
        }
    }
}
