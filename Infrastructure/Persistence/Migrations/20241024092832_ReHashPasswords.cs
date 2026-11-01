using BCrypt.Net;
using TaikoLocalServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;
using System.Text.Json;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    internal record CredentialRow(uint Baid, string Password, string Salt);

    /// <inheritdoc />
    public partial class ReHashPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Plain SQL over just the columns this migration knows, never the Credential entity: the entity gained columns
            // in later migrations (2-step verification), and querying it here failed on a new database, where those
            // columns don't exist yet ("no such column: RecoveryCodes"), so a fresh install couldn't start.
            using var context = new TaikoDbContext();
            var credentials = context.Database
                .SqlQuery<CredentialRow>($"SELECT Baid, Password, Salt FROM Credential")
                .ToList();

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

                    var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                    var password = BCrypt.Net.BCrypt.HashPassword(decodedData, salt); // Hashing the pass properly this time
                    context.Database.ExecuteSql($"UPDATE Credential SET Password = {password}, Salt = {salt} WHERE Baid = {credential.Baid}");
                    Console.WriteLine("ReHashed password for baid " + credential.Baid + " (out of " + credentials.Count + " baids)");
                }
            }
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
