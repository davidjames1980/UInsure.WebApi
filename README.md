# UInsure.WebApi — Technical Challenge undertaken by David James

## Overview

Thank you for the challenge - I very much enjoyed it and I believe I have covered all the requirements.  

Please note, I have used a stream-of-conscienceness commenting approach due to it being a coding challenge.  This is not how I would normally comment for enterprise code.

Additionally, there are two contributors to this repo, both of whom are me.  I didn't realize it until too late that my personal dev box VS config was still using an old GitHub account I had created when at ACS.  I'd normally use GitKraken but due to it being a tech challenge, no need for branch management, I used the VS git tooling.

---

## Tech Stack

- **.NET 8** — Runtime
- **ASP.NET Core Web API** — API framework
- **Entity Framework Core** — ORM
- **AutoMapper** — Object mapping
- **NUnit / Moq / FluentAssertions** — Testing

---

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Running the API

Once cloned, opened in Visual Studio or Rider, Nuget packages downloaded

The Swagger endpoint can be found at http://localhost:5298/swagger/index.html
The API can be found at http://localhost:5298/

Alternately

```bash
dotnet restore
dotnet run --project src/UInsure.WebApi.DavidJames
```

### Running the Tests

I have included the relevant nuget packages to allow running tests in IDE but alternately

```bash
dotnet test
```

There is also a full suite of endpoint tests in UInsure.WebApi.DavidJames.http

---

## Design Decisions

So without making a full enterprise solution over the weekend, I still want to get it close even if it is just going to be a discussion starter.

- **Repository pattern** — Abstracted EF Core behind `IPolicyRepository` to keep the service layer testable without a database dependency. I've used explicit methods so the call loads only the related entities it actually needs.  Also means I can actually unit test this beast via DI.

- **TDD** — I took a flexible TDD approach. Used AI to scaffold tests based on the Coding Challenge.  More below. I opted to use NUnit, Moq and FluentAssertions. I know NUnit is very reliable, popular and well-supported - I would have been perfectly happy using xUnit as it is what Microsoft use.  It was just a coin toss really and simply down to preference on a personal project with limited time.

- **AutoMapper** — Used to keep mapping boilerplate out of the service layer. In a larger solution I'd consider using it with profiles in a dedicated mapping project as they can become  massive.  Much less complex and massive than newing up in the service layer however.

- **Validation in the service layer** — Business rules (age, date windows, policyholder count) live in the service rather than as data annotations, as they are business concerns rather than input format concerns.

- **AsNoTracking on read queries** — Applied to repository methods that are purely read-only to avoid unnecessary change tracking overhead. Most calls to get the policy are readonly and tracked calls can really drag down performance over time.

- **OpenAPI/Swagger** — I've been a big fan of swagger/swashbuckle since .NET 4 days and I have wrote a lot of API! In .NET Core it is scaffolded by default which is fab.

- **DateTime handling** — As is standard whenever I'm wanting 'now' I use Datetime.UtcNow.  This is engrained in me as we can fall into the trap of assuming every consumer will be in our timezone.  UTC is the same everywhere. 

---

## AI tooling used

I used Claude AI to improve my productivity for 'chores'

- I fed the Appendix of the tech assessment in and prompted it to generate POCO classes (Policy, PolicyHolder etc).  It wasn't far off, maybe 90% correct.  Once I'd amended the issues this formed the basis of my data structure.
- I gave Claude the Background section of the tech assessment and prompted it to generate empty Given-When-Then tests with Arrange, Act, Assert comments.  It did a really decent job but as the project progressed I found I needed more.
- I used Claude to create this nice readme.md template for me.
- Once I was happy with everything I prompted Claude to create the UInsure.WebApi.DavidJames.http

---


## If I Had More Time

Tempus fugit!

- Indexing on the EF Core entities other than Policy
- More stringent use of 'required' keyword in my models.  This was a time/effort balance.  My unit tests would have needed a lot more 'fluff' which felt unsuitable for a coding challenge.
- Add a AddJsonOptions so the client could use a string for the PaymentType enum rather than int
- Add DockerFile's so we can containerize and deploy
- Split the IPolicyRepository into IReadPolicyRepository and IReadWritePolicyRepository following the 'I' in SOLID 
- Change DateTime to DateTimeOffset everywhere for a little extra safety with timezones
- Setup a shared Postman config alongside UInsure.WebApi.DavidJames.http which the whole team can use
- Add FluentValidation for request validation rather than relying purely on checks in the service. An example would be checking the postcode is legit before it hits the service.
- Add logging for errors (Serilog or old school NLog would do the job).  
- Add an IClock abstraction and inject it into the service, so that DateTime.UtcNow calls are testable without relying on real system time
- Add OpenAPI/Swagger comment generation in order to make the Swagger endpoint self-documenting
  
---

## Assumptions Made

The spec was excellent but I had to make a couple of assumptions. Enterprise-wise, I would of course ask the question directly

- The policy unique reference was a biggie. So I've worked on API's where the client provides their own unique ID because that is what they want and ones where the API creates the ID.  Coin toss, I went with letting the client provide one. I kept it as a string rather than force them into int, guid etc. 
- The spec stated policies cannot be sold more than 60 days in advance — I interpreted this as the **start date** must fall within the next 60 days, not the purchase date.
- Policy holders must be **over** 16, not 16 or over — exactly 16 on the start date is treated as invalid.  I am inclined to think we would accept 16 on the day but that is the requirement so I followed it.
- On cancellation, the cancellation day itself is counted as a used day when calculating the refund.
- Although I want to keep the internationalization available, I've had to assume from Property.Postcode we are sticking to UK now hence the UK postcode validator

---

## Thank you for your time.









