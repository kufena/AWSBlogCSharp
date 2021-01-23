# A Blog API using AWS Serverless technology.

*This is a work in progress.*

This is a serverless app for a blog hosting set up.

The first part is an AWS API, written in C#, using AWS Lambda, API Gateway, and Cognito,
all described using a serverless template written in SAM/Cloudformation.

Secondly, there's an attempt at a Blazor front-end, which uses Cognito to authenticate a
user, and use the said API.

The intention of the project is that the API will expose resources under users.
So, The API will have paths

    /{user}/blog/     - GET all blog posts, and POST a new blog post.
    /{user}/blog/{id} - GET a specific blog post, or PUT an update.

Authentication will mean that a user, once logged in via the Cognito Identity Pool, will only
have permissions (via IAM roles) to see their own specific blog posts.

A separate front-end will be required for public consumption, with just a simplified GET.

CORS is enabled on the API (although it doesn't quite work - the OPTIONS returns an appropriate header,
but the GET/PUT/POST set the header in the lambda functions, which is not right.)

The back-end requires a MySql database, and an S3 bucket.  These are conveyed to the back-end via
a pair of secrets, one containing the connection details, the other the uri of the bucket.
There must be a better way to do this - it costs money, for a start!

I am not a front-end developer - never have been.  I am trying to learn Blazor, which is an up hill battle
at the moment.  So far, there is a sign-up page which is not robust, but which does allow you to register a
user, confirm using the emailed code and log in.  Won't display anything yet as not hooked up to the API.

That's all for now.
