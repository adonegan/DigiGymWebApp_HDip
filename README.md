# DigiGym: Fitness & Diet Tracking Web App

Digigym is an online fitness and diet tracking app for people who like to exerise but who don't want to visit the gym. With Digigym users can create accounts, log their workouts and health achievements and see charts that show their progress over time. The app can be accessed via https://digigym.azurewebsites.net/ (refresh a few times if you see an error as it pauses if inactive for long periods of time).

## Features

Client users can register a new account via public form, log in and take the following actions:

- Add their weight and height to calculate BMI
- Create, read, update and delete weight information
- Create, read, update and delete water intake
- See daily water intake in a doughnut chart
- Create, read, update and delete food intake
- Add calories and have a daily count
- Add macros (protein, fat, carbs) and see proportions in a pie chart
- See their food diary by date
- Create, read, update and delete workouts
- Ability to add the following workouts: walk, cycle, run, swim or other
- Log start and end times as well as effort level
- Send and receive messages from trainers
- See new messages in a list
- See a visual indicator of unread / read messages

Trainer users can register a new account using the same public form, but they must be approved by Admin users before they can log in to the portal. Trainer users can take the following actions:

- See, open messages and reply to Client users
- Trainers do not have access to Client data by design

Admin users do not sign up like Clients or Trainers but are added to the system directly. Admins can take the following actions:

- See every user in the system - name, email address and usertype only
- Upgrade trainers to admin (admins cannot upgrade or downgrade themselves)
- Downgrade other admins
- Reject trainers
- Approve trainers
- See all trainers pending approval
- Admins do not have access to Client data by design

### Nice to Have 

- **Goals:** Originally part of the initial plan, I marked this as a Won't Do mid-project due to time constraints. For the future, I would create a Goal model, add a new migration and have users establish water, weight and workout goals. Clients would see their progress in the context of reaching their goals, and I would achieve this by adding a new dashboard for charts that visually display this progress.
- **Notifications:** Always a nice to have feature, I'd like for Clients to be alerted to updates, especially for received messages from Trainers. Additionally, I'd like to set up reminder notifications as motivations for Clients to keep going with their goals and progress.

## Testing

I created approx. 70 unit tests that mainly test controller actions, views and the BMI and calorie counter services I created for the project. More detail can be found in the Testing Documentation upload on Moodle.

## Deployment

Digigym is hosted on Azure. I took the following steps to achieve this:

- Logged in to the Azure portal, updated billing
- Created a new resource group
- Created a database and server
- Created a Web App Service
- I added my AzureConnections db string and value to my secrets.json file
- Then I updated the Program.cs page to connect to the new db
- In Visual Studio > Solution > Publish to add my AzureConnection as a service dependency
- In the Azure Portal I ensured my App Service had the correct db environment variables
- Then I hit Publish in Visual Studio to publish my app

I can still test new updates and changes by swapping out AzureConnection with DefaultConnection, my test db, and switching back when it's time to Publish these changes again.
