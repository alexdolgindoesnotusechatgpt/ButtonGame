using UnityEngine;
using System.Collections.Generic; // Required for Lists and Queues
using UnityEngine.Events;

public class EmailPlaylistManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EmailTyper emailTyper;

    [Header("Content")]
    [Tooltip("Add all your emails here. They will be played in random order.")]
    [TextArea(5, 10)]
    [SerializeField] private List<string> emailContentList;

    [Header("Events")]
    public UnityEvent OnAllEmailsCompleted;

    // A Queue works like a playlist: first in, first out
    private Queue<string> playlist = new Queue<string>();

    void Start()
    {
        InitializePlaylist();
        LoadNextEmail();
    }

    // 1. Shuffle the list and put it into a Queue
    void InitializePlaylist()
    {
        // Copy the list so we don't mess up the inspector order
        List<string> shuffledList = new List<string>(emailContentList);

        // Fisher-Yates Shuffle Algorithm (The standard way to shuffle cards)
        for (int i = 0; i < shuffledList.Count; i++)
        {
            string temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // Add them to the queue
        foreach (string email in shuffledList)
        {
            playlist.Enqueue(email);
        }
    }

    // 2. Called to start the next email
    public void LoadNextEmail()
    {
        if (playlist.Count > 0)
        {
            string nextEmail = playlist.Dequeue();

            // Send it to the Typer
            emailTyper.SetNewEmail(nextEmail);

            Debug.Log($"Loaded new email. {playlist.Count} remaining.");
        }
        else
        {
            Debug.Log("Playlist empty! You win!");
            OnAllEmailsCompleted?.Invoke();
            // Optional: Disable typer cursor here
        }
    }

    public void LoadNextEmailWithDelay()
    {
        // Wait 1.0 seconds, then run LoadNextEmail
        Invoke(nameof(LoadNextEmail), 1.0f);
    }
}