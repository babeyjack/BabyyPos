# BabyyPos

A basic Sever-Client Point-of-Sale system

This project is being made for my computer science NEA for A-Level.
I created a simple Point-Of-Sale Till system which could be used in a professional environment.

My project was created in Unity Engine and utilised Networking and SQLite to create a Server-Client connection with database access.

Originally this repository contained the Unity Files for the project. This was proving to be inefficient to keep uploading the files. I also ran out of LFS space to be able to do this. However, I will now be adding any Test/Final builds that I create (fully functioning .exe files) and adding the final versions of any scripts that I have created.

Below is a quick guide about how to use this Repository:

## NEA_File

This is the main documentation for my project. This includes the analysis of the problem, my initial design to fix the problem, developing my solution and then a conclusion. To view the Word file, open it with your word processor of choice, ensuring that it is compatible with .docx files. Alternately you can open the PDF version with your PDF viewer of choice. There is a contents page, a bibliography and a definitions page included in the document as well.

## Scripts

In this repository there is a file called Scripts. These are the most recent versions of the C# scripts that I wrote for my project. All of these scripts are fully commented and the variables have been grouped and headed to make it easier to read. All the scripts are in their respective files as outlined in the NEA_File, however, it should be self explanatory which Scripts are where.

## Builds

In this repository there is a file called Builds. This file contains the most recent builds for my project. Inside each version file there are 3 .exe shortcuts. One for the Server build and one for the Client build. **Please Note:** you must have a Server running for the Client to connect to, otherwise the Client build will not work. This means that you may have to run both the Server and the Client at the same time. I have also included a Server-Client build, which is the Client build but it also generates a server. This is useful for testing.

All builds are limited to only 2 Client connections, and a set number of database entries. I hope you can understand this is so it cannot be used commercially.

Both of the Server builds will automatically create the relevant files it requires. It will also automatically add some data to the Database, to enable you to test if all of the features are working correctly. The items of data included are outlined in the NEA_File.

In the Builds folder there is also a change log. This contains all of the changes that have been made in between versions. So if you would like to see what has changed between the version you last tried and this version you can see.

## Testing

If you would like to test the system out, I have included the database file which I used during development. To use this file, copy and paste it into the following directory C:\Users\[User]\AppData\LocalLow\DefaultCompany\BabyyPos replacing [User] with the current users name
NOTE: This project has only been compiled for Windows

## Post-Script

This project was solo developed, I plan to update this project every now and again when I get time. If you find any bugs please report it, as this can help me know what I need to fix. If you have any issues with the project, don't be afraid to get in contact, I'd be happy to help.
