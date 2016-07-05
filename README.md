# SimpleWeb
It implements register, sign in, sign out, forgot password/reset password, and email send functionalities.

The controller (AccountController) in this project accesses the service layer (/Services/AccountServices)to execute the actions. This approach helps in achieving modularity to a degree in this project.

Getting started: 
Create an account by clicking on Register link in navigation bar above. 
You'll need an email service to activate your account!
I used my gmail account to send emails. Go over to Web.Config file in the project and scroll down to system.net section and hook up your email service.


Meanwhile...
I've created an account for you to use. Go over to login page and use:
Email: guest@user.com
Password: Gu3st@user.com

No database? No Worries:: 
I've attached a database file with the project (which can be found under /SimpleWeb/App_Data directory) which saves all the user info. All you need is Visual Studio 2015 to run it!
Note: The compatibility level of database file attached is now updated to 100 from 130.

Protected content/resource: 
The only content/resource right now which needs authorization in this project is the "About" page.
