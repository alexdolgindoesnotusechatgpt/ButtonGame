using UnityEngine;
using System.Collections;

public class ChattyEmployee : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Prefab for the chat bubble (must have ChatBubble script)")]
    [SerializeField] private GameObject chatBubblePrefab;
    
    [Tooltip("Drag a BoxCollider2D here to define where bubbles can appear")]
    [SerializeField] private Collider2D spawnArea;
    
    [SerializeField] private float spawnInterval = 0.8f;

    private Coroutine spawnCoroutine;

    public void StartChattering()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopChattering()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        
        // Cleanup existing bubbles so they don't linger
        if (spawnArea != null)
        {
            foreach (Transform child in spawnArea.transform)
            {
                if (child.GetComponent<ChatBubble>())
                    Destroy(child.gameObject);
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnBubble();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBubble()
    {
        if (spawnArea == null || chatBubblePrefab == null) return;

        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        
        Vector3 spawnPos = new Vector3(x, y, 0);
        // Parent the bubble to the spawn area so we can clean it up easily
        Instantiate(chatBubblePrefab, spawnPos, Quaternion.identity, spawnArea.transform);
    }
}