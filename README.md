# Vulindlela — Student Starter Pack

Everything here is supplied to you. Nothing here is marked. It exists so that
you spend your six weeks on the interesting problems rather than on setup.

## What is in this pack

| Path | What it is | What to do with it |
|---|---|---|
| `legacy/LegacyAppointmentManager.cs` | A real class from BranchDesk 2011 | **Analyse it** in Deliverable 1. Do not fix it, compile it or run it. |
| `docs/adr/0000-template.md` | The Architecture Decision Record template | Copy it three times. Fill it in. |
| `docs/adr/EXAMPLE-0001-choosing-sqlite.md` | A worked ADR, on a decision you are not asked to make | Read it to see the standard. Do not copy its content. |
| `.gitignore` | A .NET gitignore | Put it at the root of your repository on day one. |
| `SETUP.md` | Environment setup | Follow it before you write any code. |

## Your first hour

1. Install the .NET 8 SDK. Check with `dotnet --version`.
2. Create a **private** GitHub repository. Add your lecturer as a collaborator.
3. Copy `.gitignore` to the root and commit it **first**. This is what stops
   your `.db` files and `bin/` folders being committed.
4. Read the legacy class. Write down every problem you can see before you read
   anything else. That list is the start of Deliverable 1.

## A word on the legacy class

It is not a puzzle and there is no trick. It is an ordinary example of what
happens when a file is edited by six people over twelve years and nobody is
allowed to restructure it. Almost every graduate developer inherits something
like it in their first job.

Read the comments in it. The 2019 maintainer explains exactly why the SMS code
is in the wrong place, and the reason is one you will hear again in industry.
