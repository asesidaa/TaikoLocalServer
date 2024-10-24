using BCrypt.Net;
using GameDatabase.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;
using System.Text.Json;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ReHashPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using var context = new TaikoDbContext();
            var credentials = context.Credentials.ToList();

            foreach (var credential in credentials)
            {
                // Check if the password is empty or null
                if (!string.IsNullOrEmpty(credential.Password))
                {
                    // Passwords are currenrly stored as a string containing Password + Salt encoded in base64 4 times.
                    // This is unacceptable so we'll rehash everything with bcrypt.
                    var decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(credential.Password)); // First pass
                    decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(decodedData)); // Second pass
                    decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(decodedData)); // Third pass
                    decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(decodedData)); // Last pass, leaving us with plain text password + salt
                    decodedData = decodedData.Substring(0, decodedData.Length - credential.Salt.Length); // Recovering plain text password

                    credential.Salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                    credential.Password = BCrypt.Net.BCrypt.HashPassword(decodedData, credential.Salt); // Hashing the pass properly this time
                    Console.WriteLine("ReHashed password for baid " + credential.Baid + " (out of " + credentials.Count + " baids)");
                }
            }
            context.SaveChanges();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // On Down we reset all passes and salts for everyone.
            migrationBuilder.Sql(@"
                UPDATE Credential
                SET Password = '',
                    Salt = ''
            ");
        }
    }
}
