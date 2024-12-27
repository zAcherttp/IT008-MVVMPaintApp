using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMPaintApp.Models
{
    internal class ProjectNameAvailabilityAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var projectName = value as string;

            if (string.IsNullOrWhiteSpace(projectName)) return ValidationResult.Success!;

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolderPath = Path.Combine(documentsPath, "MyPaint");
            string projectPath = Path.Combine(appFolderPath, projectName);

            return Directory.Exists(projectPath) ? new ValidationResult(ErrorMessage) : ValidationResult.Success!;
        }
    }
}