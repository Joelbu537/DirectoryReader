# Planned Features
## Methods and Routine
### Methods
- WriteCurrent   writes the current directory, its subdirectories and files, admin permissions and the clipboard
- GetInput       writes the current volume + the current directory and accepts inputs
- HandleInput    computes 1 input at aa time
### Routine
WriteCurrent();
while(true){
  GetInput();
  while(Queue.Length != 0){
    HandleInput();
  }
}
## Multi-Command usage
Execute multiple commands in a row
- copy data.txt | copy data2.txt
The commands get added to a queue  
A while-loop will work off each command before presenting any results
## Commands
### En / De -crypt
- en/decrypt [nothing] Encrypts the current directory
- en/decrypt [all]     Encrypts the current directory and all subdirectories
- en/decrypt [file]    Encrypts a specific file
### cd
- cd [subdirectory]    jumps to a subdirectory in the current directory
- cd [..]              steps one directory up
### admin
- admin                restarts the program with admin permissions if you want to cmake changes to protected files. You need permissions to execute applications as admin!
### copy
- copy [file]          Adds the file to the copy-list
- copy [folder]        Adds all files in the folder to the copy-list
- copy [all]           Adds all files in all sub-directories to the copy-list
- copy [nothing]       Adds all files in the current directory to the copy-list
### paste
- paste                pastes all files from the copy-list in the current directory
### copyclear
- copyclear            clears the copy-list
### help
- help                 lists all commands
      copy
      [file] Adds a single file to the clipboard
      [all] ...
      admin  -  bla bla bla
### delete
- delete [file]        deletes a specific file
- delete [folder]      deletes a specific folder
- delete [all]         deletes everything in the current directory
### execute
- execute [argument]   executes an application or http command simmilar to WIN + r
### volume
- volume [volume]      changes the current volume (ex: volume C changes the working directory to C:\\)
### clear
- clear                clears the console
### ls
- ls                   calls WriteCurrent();

## Everything is subject to change!!
