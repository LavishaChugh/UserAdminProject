﻿namespace TheProject.Dtos
{
    public class UpdateUserDto
    {
        public Guid Id { get; set; } 
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Dob { get; set; } = string.Empty;

    }
}
