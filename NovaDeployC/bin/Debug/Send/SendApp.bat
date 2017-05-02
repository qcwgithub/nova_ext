REM 说明
REM type：sendApp表示发送端的App，必须有
REM cfg：当type=SendApp时必须配置，表示服务器配置文件，每台电脑的ip，及每台电脑上部署的游戏服务器的名字（没有服务器ID）

REM action：要做什么，有2种，copyFile和copyDir
REM file：当action=copyFile时必须配置，表示要复制哪个文件；路径相对于当前路径
REM targetServers：可以赋值为all，表示所有的电脑、所有的服务器；
REM targetServers：如果不是all，那么需要列出所有服务器的名字，以逗号隔开，不可以有空格。例如srv1,srv2，每个服会自动查找所在的电脑
REM targetFile：当action=copyFile时必须配置，表示要复制到服务器上的哪个文件路径，相对于RecvApp所在的目录；在这个参数中可以使用{SERVERNAME}表示服务器目录名
REM targetDir：当action=copyDir时必须配置，表示要复制到服务器上的哪个文件路径，相对于RecvApp所在的目录；在这个参数中可以使用{SERVERNAME}表示服务器目录名

REM 以下这句，把当前目录下的activity.csv复制到所有电脑的RecvApp所在目录\config\activity.csv
REM ..\NovaDeployC.exe type=sendApp cfg=wdjSettings.txt action=copyFile file=activity.csv targetServers=all targetFile=config\activity.csv

REM 以下这句，把当前目录下的activity.csv复制到所有电脑的RecvApp所在目录下的【每个】服务器\config_clone\activity.csv
REM ..\NovaDeployC.exe type=sendApp cfg=wdjSettings.txt action=copyFile file=activity.csv targetServers=all targetFile={SERVERNAME}\config_clone\activity.csv

REM 以下这句，把当前目录下的Mainland1目录得到到srv1所在电脑的RecvApp所在目录下的Mainland1目录
REM ..\NovaDeployC.exe type=sendApp cfg=wdjSettings.txt action=copyDir dir=Mainland1 targetServers=srv1 targetDir=Mainland1

REM echo 77777777
REM pause

