using UnityEngine;
using System.IO;

public class ImageLoader
{
    private string[] paths;
    private bool[] pathWasUsed;
    private string dir;
    int next;

    public void InitWithFolder(string directory)
    {
        dir = directory;
        paths = Directory.GetFiles(dir);
        pathWasUsed = new bool[paths.Length];
        next = -1;
    }

    public Texture2D LoadNextImage(bool randomized)
    {
        if (randomized)
        {
            next = GetNextShuffuledIndex();
        }
        else
        {
            next = GetNextIndex();
        }

        var bytes = File.ReadAllBytes(paths[next]);
        Texture2D texTmp = new Texture2D(2, 2, TextureFormat.DXT1, false);
        texTmp.LoadImage(bytes);
        return texTmp;
    }

    //May want to make a non shuffled version
    private int GetNextShuffuledIndex() 
    {
        var random = new System.Random();
        var next = random.Next(0, paths.Length);
        int attemptsAllowed = 10;
        int attempts = 0;

        while (attemptsAllowed > attempts && pathWasUsed[next] == true) //Random attempts
        {
            next = random.Next(0, paths.Length);
            attempts++;
        }
        if (pathWasUsed[next] == true) //If still no path, interate linearly
        {
            for (int i = 0; i < pathWasUsed.Length; i++)
            {
                if (pathWasUsed[i] == false)
                {
                    next = i;
                    break;
                }
            }
        }
        if (pathWasUsed[next] == true) //We have no unused images, start over
        {
            InitWithFolder(dir);
            next = random.Next(0, paths.Length);
        }

        pathWasUsed[next] = true;

        return next;
    }

    private int GetNextIndex()
    {
        next = next++ >= paths.Length-1 ? 0 : next++;
        pathWasUsed[next] = true; //In case shuffling is enabled later

        return next;
    }

}
