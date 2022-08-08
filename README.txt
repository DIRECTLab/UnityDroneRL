Go through the instalation process for unity, and the unity packages as described 
here: https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Installation.md

Python packages are outlined in the requirements.txt file, use this to set up
the virtual environment.

Once all that is done you can open the project from unity, navigate to the example-rl scene and open it.

From the command line activate the virtual environement in the project folder, and then run ml-agents 
learn, next you need to activate the project from the unity side byclicking play in the editor to run 
the scene. This will start the learner. This learner doesnt take too long to learn well, and results 
can be seen in the log files, or as output in the command line.

Run ml-agents --help to see other parameters you can add to the learning process, like config files
and output logs.
