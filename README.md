# Vulindlela — Government Services Platform

 # Project Overview

   The limitations of the legacy government appointment booking system have been addressed by Vulindlela by separating booking, document clearance and citizen-facing services into distinct components. With Vulindlela, citizens can book government service appointments, track the clearance status of their documents and access their personal records, and authenticated, authorized, and logged audit actions.

   Developed for the Eduvos ITEHA3-44 Software Engineering assignment.


 # Technology Used

   - C# and .NET 10
   - ASP.NET Core Web API and ASP.NET Core MVC and Razor Views
   - Entity Framework Core and SQLite
   - Cookie authentication and JWT bearer authentication
   - ASP.NET Core password hashing
   - xUnit and ASP.NET Core integration testing
   - Git and GitHub


# Solution Structure

The solution comprises the following projects:
   - Vulindlela.Booking.Api – Appointment booking, branch availability and concurrency protection and API access auditing
   - Vulindlela.Clearance.Api – Document clearance and asynchronous verification and booking notifications
   - Vulindlela.Portal – Authentication, appointment management and access history
   - Vulindlela.Shared – Contracts and common types
   - Vulindlela.Booking.Tests – Automated booking and API security tests


# System Architecture

   Responsibilities for subsystems are separated.

   The Booking API follows a three-tier architecture:
   - The Presentation tier, which consists of controllers, exposes HTTP endpoints and enforces authorization on request.
   - The Business Logic tier, consists of services, which validate booking requests and check availability, and apply booking rules.
   - The Data Access tier, consists of repositories and Entity Framework Core, which persists booking to the SQLite database.
   - The Clearance API processes verification work asynchronously using a background worker and channel based queues.

   The Portal uses named HTTP clients to communicate with the APIs.

# Prerequisites

Install:

- .NET 10 SDK
- Git
- Visual Studio Code or Visual Studio

# Clone and Build

git clone https://github.com/Dillon-Fourie/Vulindlela.git
cd Vulindlela
dotnet restore
dotnet build

# Configure JWT Authentication

When the Portal makes authenticated calls to the Booking API, it receives short-lived JWTs on behalf of the user. Both services have to use the same signing key.
The key is stored in .NET user secrets and not committed to source control.

Initialise User Secrets:
   dotnet user-secrets init --project .\src\Vulindlela.Portal
   dotnet user-secrets init --project .\src\Vulindlela.Booking.Api  

A signing key is generated and saved in an environment variable on your machine. 
   $keyBytes = New-Object byte[] 64
   $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
   $rng.GetBytes($keyBytes)
   $rng.Dispose()
   $jwtKey = [Convert]::ToBase64String($keyBytes)

   dotnet user-secrets set "Jwt:Key" $jwtKey --project .\src\Vulindlela.Portal
   dotnet user-secrets set "Jwt:Key" $jwtKey --project .\src\Vulindlela.Booking.Api

   Remove-Variable jwtKey, keyBytes, rng

This key must be kept private. Each developer has to generate their own key for local development.
For local development, use the provided launch profile for the Development environment, which loads User Secrets.

# Run the Applications

Open three terminals from the repository root.

   Terminal 1: Booking API
   Address: http://localhost:5112

   Terminal 2: Clearance API
   Address: http://localhost:5165

   Terminal 3: Citizen Portal
   Address: http://localhost:5138

   Open http://localhost:5138 in your browser.

Applications initialize their local SQLite databases and seed sample data.

# Demonstration Accounts

Username  - Password        - Role     - Citizen ID
citizen1  - Citizen1!Demo   - Citizen  - TEST-CITIZEN-005
citizen2  - Citizen2!Demo   - Citizen  - TEST-CITIZEN-006
official1 - Official1!Demo  - Official - N/A

# Booking Functionality

The Booking API provides:

List of Branches
   - Available appointment slots
   - Creates appointment slots
   - Retrieves appointment slots
   - Citizen appointment history
   - Double-booking is prevented

Three demonstration branches are provided:
   - Cape Town Civic Services
   - Johannesburg Civic Services
   - Durban Civic Services

Each branch has 14 appointment slots for weekdays in the future. Each slot is 30 minutes, beginning at 9 am and ending at 4 pm.

Booking API Endpoints:

   Method - Endpoint                              - Purpose
   GET    - /api/branches                         - Get branches
   GET    - /api/branches/{branchId}/slots        - Get branch slots
   POST   - /api/appointments                     - Create appointment
   GET    - /api/appointments/{appointmentId}     - Get appointment
   GET    - /api/appointments/citizen/{citizenId} - Get citizen appointments
   GET    - /api/accesslogs/citizen/{citizenId}   - Get citizen access logs

Appointment and audit endpoints require JWT. Citizens are only allowed to view their own appointment details, while officials are allowed to view all appointment records.

Example Booking Request
   {
  "slotId": 1,
  "citizenId": "TEST-CITIZEN-005",
  "citizenIdNumber": "9001015009087",
  "citizenName": "Test Citizen",
  "serviceType": "ID Application"
   }

The slot ID should correspond to an available future appointment slot. When making an appointment through the Portal, a citizen is restricted to the appointment record for his/her/their authenticated citizen ID.

# Preventing Double Bookings

Every appointment slot is restricted to one user.

The Booking API implements this business rule and ensures database uniqueness through the following mechanisms:
   - Business Rule Availability checks
   - Database uniqueness constraint on Appointment.SlotId
   - Competing Request Handling

The database uniqueness constraint ensures that concurrent requests for the same appointment slot cannot succeed.

The rationale for the design is explained in detail in docs/adr/0002-preventing-double-booking.md.

# Document Clearance

The Clearance API is responsible for handling document clearance requests and checking the status of documents.

Its processing involves
   - Receiving a document clearance request
   - Marking the request as pending
   - Queueing the request for verification
   - Performing verification asynchronously
   - Marking the request as verified or not verified

The Booking API creates appointments and notifies the Clearance API. Separating the clearance and appointment processing services is deliberate. If the clearance processing service fails, appointment booking should still be allowed.

# Authentication and Authorisation

The Portal uses cookie-based authentication. Passwords are hashed using ASP.NET Core. The Booking API uses JWT bearer token authentication.

Other controls include:
  - Authentication before booking
  - Role-based permissions for citizens and officials
  - Citizen ownership checks on appointment records
  - JWT verification for protected Booking API endpoints
  - Reject unauthenticated requests
  - Access-denied responses for unauthorized record access

# Access Auditing

Both the Portal and Booking API log citizen record access.

Access audit records include:
  - Citizen ID
  - Username of the record access user
  - User role
  - UTC timestamp
  - Reason for access
  - Resource type and identifier

The Portal combines its own audit records with audit records from the Booking API. Citizens can view their access audit records. Officials can view access audit records for citizens they are authorized to access.

# Automated Testing

There are 12 xUnit testing tests as part of the solution.

   The tests cover:
   - Booking functionality
   - Concurrency protection
   - API security

   Security integration tests include:
   - Unauthenticated requests result in a status code 401.
   - Citizens cannot access another citizen’s appointments.
   - Officials can access citizen appointments.
   - Successful authorized reads create audit records.

The security integration tests use a separate in-memory SQLite database and test-only authentication identities.

# Run Tests

From the repository root:
   dotnet test

Latest verified result: 12 passed, 0 failed, 0 skipped.

# Suggested Demonstration

  - Launch the three apps.
  - Sign in as citizen1.
  - Check the branches and appointment slots.
  - Make an appointment.
  - Go to My Appointments to view the appointment.
  - Go to Access Log to view the logged access.
  - Sign out and sign in as citizen2.
  - Try to access citizen1’s record to see if access is denied.
  - Sign in as official1 and view citizen appointment records.
  - Run dotnet test and demonstrate that all tests are passing.

# Architecture Decision Records

The docs/adr directory contains the architectural decision records prepared for the assignment.

These include the service boundary, double-booking prevention, and other architectural decisions along with alternatives and trade-offs.

Other initial configuration info is in SETUP.md.

# Git and Development Workflow

Development was done using feature branches and pull requests.

Pull requests covering Authentication and Authorisation are included.

Later pull requests include Booking API security and access auditing.

Git history shows the feature branch workflow and integration into main.

# Known Limitations

Currently SQLite is used only for local development, demonstration and test databases. SQLite should NOT be used for production-scale database deployments.

Development accounts are locally seeded demonstration accounts.

JWTs keys are stored locally and shared between the Portal and the Booking API via user secrets.

Local HTTP endpoints are development endpoints and must NOT be used for production. Development endpoints must be secured using SSL.

Cleared verification is hard coded and is NOT connected to a government authority.

The Vulindlela solution does NOT use a real national identity provider.

The Portals and the Booking APIs audit events are stored separately and merged for display.

The Booking API database uses an incremental table-initialization approach; in a production environment, a consistent EF Core migration strategy should be used.

Some integrations use best-effort communication, whereas production systems should use communication with guaranteed delivery.

# Project Status

The Vulindlela solution implements a layered Booking API, MVC Portal, authentication, authorization, and access auditing as required by the assignment. It also implements automated testing and the GitHub feature branch workflow.

The application is a proof-of-concept and should NOT be used for production government services.