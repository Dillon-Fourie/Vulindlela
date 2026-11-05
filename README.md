# Vulindlela — Government Services Platform

 # Project Overview

This is a software engineering project developed as part of the Eduvos ITEHA3-44 assignment.
This project is an attempt to provide a modular and flexible architecture to the application to automate the processes of government services.
The application comprises modules for appointment booking, clearance processing, and a citizen portal.


 # Technology Used

   C# and ASP.NET Core Web API

   .NET 10

   Entity Framework Core

   SQLite

   xUnit

   Git and GitHub


# Solution Structure

The solution comprises the following projects:

   Vulindlela.Booking.Api - This project manages bookings by citizens for specific time slots for various government services.

   Vulindlela.Clearance.Api - Processes clearances.

   Vulindlela.Portal - Citizen Portal

   Vulindlela.Shared - Shared Contracts and Common Types

   Vulindlela.Booking.Tests - Automated tests for the Booking Module


# The Booking Module is implemented using a layered architecture.

 # Presentation Layer
   This layer defines a set of endpoints for the citizens to book various government services.

 # Business Logic Layer
   The layer validates the citizens' booking requests, and determines the availability of the requested appointment time. The layer also handles various booking conflicts.

 # Data Access Layer

 The layer uses Entity Framework Core to access the Booking Module's SQLite database.

# Getting Started

 # Prerequisites
   Installing .NET 10 SDK
   Git
   Visual Studio Code


# Copy the Repository

   git clone https://github.com/Dillon-Fourie/Vulindlela.git
   cd Vulindlela

# Build the Solution
   dotnet build

# Run Booking API
   dotnet run -p src/Vulindlela.Booking.Api

During development, the Booking API is accessible at: http://localhost:5112

The application automatically makes a SQLite database at start up if the database doesn't already exist.

# Database Seeding

The Booking API automatically creates 3 branches:
   Cape Town Civic Services
   Johannesburg Civic Services
   Durban Civic Services

For each branch, the application generates appointment slots for 14 weekdays in the future.
Appointments are 30-minute long and available from 09:00 to 16:00, on weekdays only.
The seeding process looks to see if there are already slots booked for that time, and will not generate a duplicate slot.

# Booking API Endpoints

   GET - /api/branches - Get a list of all branches
   GET - /api/branches/{branchId}/slots - Get all available appointment slots for the given branch
   POST - /api/appointments - Book an appointment
   GET - /api/appointments/{appointmentId} - Get an appointment

# Example Booking Request
   POST /api/appointments

   {
   "slotId": 1,
   "citizenId": "test-citizen-001",
   "citizenIdNumber": "9001015009087",
   "citizenName": "Test Citizen",
   "serviceType": "ID Application"
   }

Replace slotId with an available future appointment slot ID.
The API returns:
   201 Created for successful bookings.
   400 Bad Request for invalid booking information.
   404 Not Found if the requested appointment is already booked, or if the requested appointment slot does not exist.
   409 Conflict for duplicate bookings.

# Preventing Double-Bookings

   Each appointment slot is only allowed to have 1 citizen booked.
   The Booking API persists appointments and uses a unique SQLite index on Appointment.SlotId to prevent duplicate appointments.
   The Business Logic Layer (along with a unique constraint) performs availability checks to ensure the slot is not already booked.
   Conflicting bookings are returned by the API as a booking error.
   This design choice along with alternatives are documented in:
   docs/adr/0002-preventing-double-booking.md

# Automated Testing

The Booking API uses xUnit and in-memory SQLite databases.

The tests verify:
      Successful appointment bookings.
      Rejection of bookings for slots that don’t exist.
      Rejection of bookings for slots that are already taken.
      Rejection of bookings for sequential duplicate slots.
      Concurrent bookings for the same slot.
      Uniqueness of appointments at the database level.

# Run the Tests

   dotnet test tests/Vulindlela.Booking.Tests

   The test fixture creates separate SQLite connections with a shared in-memory database, allowing concurrent operations without a non-thread-safe connection object.
   At the time of the Deliverable 2 review, all 7 tests passed, including the original starter test.

# Architecture Decision Records

Decisions on architecture are documented in the docs/adr directory.
The ADR on double booking explains the rationale to ensure appointment uniqueness at the database level, and offers alternatives and trade-offs.

# Additional Setup

Setup for initial assignment is in the SETUP.md file, and offers additional technical information.

# Development Status

Deliverable 2: The Booking API is developed, and its automated tests are completed. The bookings API is waiting final submission verification.

The Clearance API and citizen portal are additional components of the Vulindlela assignment and are not described herein as completed functionality.