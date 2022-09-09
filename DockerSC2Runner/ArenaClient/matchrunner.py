import os

print("matches started")

bot1 = "basic_bot"
race1 = "T"
type1 = "python"

bot2 = "loser_bot"
race2 = "T"
type2 = "python"

mapList = ["2000AtmospheresAIE", "BerlingradAIE", "BlackburnAIE", "CuriousMindsAIE", "GlitteringAshesAIE", "HardwireAIE"]
mapIndex = 0

for x in range(10):
    matchString = bot1 + "," + race1 + "," + type1 + "," + bot2 + "," + race2 + "," + type2 + "," + mapList[mapIndex]
    f = open("matches", "w")
    f.write(matchString)
    f.close()
    print("match " + str(x) + ": " + matchString)
    os.system('cmd /c "docker-compose up"')
    mapIndex = mapIndex + 1
    if mapIndex >= mapList.__len__():
        mapIndex = 0

print("matches ended")