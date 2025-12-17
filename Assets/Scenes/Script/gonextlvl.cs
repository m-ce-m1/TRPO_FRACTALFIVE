using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [Header("Настройки перехода")]
    public string nextSceneName = "lvl2"; // Имя следующей сцены
    public Color triggerColor = Color.green; // Цвет триггера для визуализации
    public float checkDelay = 0.5f; // Задержка между проверками
    
    [Header("Визуальные эффекты")]
    public ParticleSystem transitionParticles;
    public AudioClip transitionSound;
    
    private GameObject player;
    private bool isTransitioning = false;
    private float lastCheckTime = 0f;
    private Renderer triggerRenderer;
    private AudioSource audioSource;
    private Material originalMaterial;
    
    void Start()
    {
        // Находим игрока
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Игрок не найден! Убедитесь что у игрока есть тег 'Player'");
        }
        
        // Настраиваем визуализацию триггера
        triggerRenderer = GetComponent<Renderer>();
        if (triggerRenderer != null)
        {
            originalMaterial = triggerRenderer.material;
            
            // Создаём материал для подсветки
            Material highlightMat = new Material(Shader.Find("Standard"));
            highlightMat.color = triggerColor;
            highlightMat.SetFloat("_Mode", 3); // Transparent режим
            highlightMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            highlightMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            highlightMat.SetInt("_ZWrite", 0);
            highlightMat.DisableKeyword("_ALPHATEST_ON");
            highlightMat.EnableKeyword("_ALPHABLEND_ON");
            highlightMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            highlightMat.renderQueue = 3000;
            triggerRenderer.material = highlightMat;
        }
        
        // Добавляем AudioSource если нужен звук
        if (transitionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = transitionSound;
        }
        
        // Делаем объект триггером если он ещё не является им
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
        
        Debug.Log($"Триггер перехода уровня создан. Следующий уровень: {nextSceneName}");
    }
    
    void Update()
    {
        // Периодически проверяем игрока если он рядом
        if (!isTransitioning && player != null && Time.time - lastCheckTime > checkDelay)
        {
            lastCheckTime = Time.time;
            CheckPlayerDistance();
        }
        
        // Плавное мерцание триггера
        if (triggerRenderer != null)
        {
            Color currentColor = triggerRenderer.material.color;
            float alpha = 0.3f + Mathf.PingPong(Time.time * 0.5f, 0.3f);
            triggerRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }
    
    void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        // Если игрок достаточно близко
        if (distance < 2f)
        {
            StartTransition();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Проверяем если это игрок
        if (!isTransitioning && other.CompareTag("Player"))
        {
            StartTransition();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // На случай если триггер не настроен
        if (!isTransitioning && collision.gameObject.CompareTag("Player"))
        {
            StartTransition();
        }
    }
    
    void StartTransition()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        Debug.Log($"Начинается переход на уровень: {nextSceneName}");
        
        // Визуальные эффекты
        if (transitionParticles != null)
        {
            transitionParticles.Play();
        }
        
        // Звуковой эффект
        if (audioSource != null && transitionSound != null)
        {
            audioSource.Play();
        }
        
        // Меняем цвет триггера
        if (triggerRenderer != null)
        {
            triggerRenderer.material.color = Color.white;
        }
        
        // Сохраняем прогресс (можно сохранять здоровье, предметы и т.д.)
        SavePlayerProgress();
        
        // Загружаем следующую сцену с задержкой
        Invoke("LoadNextScene", 1f);
    }
    
    void SavePlayerProgress()
    {
        // Здесь можно сохранять данные игрока
        // Например, здоровье, стамину, инвентарь
        
        PlayerController playerController = player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Пример сохранения здоровья
            PlayerPrefs.SetFloat("PlayerHealth", GetPlayerHealth(playerController));
            PlayerPrefs.SetFloat("PlayerStamina", GetPlayerStamina(playerController));
            PlayerPrefs.SetString("CurrentLevel", nextSceneName);
            PlayerPrefs.Save();
            
            Debug.Log("Прогресс сохранён");
        }
    }
    
    float GetPlayerHealth(PlayerController playerController)
    {
        // Получаем здоровье через рефлексию или публичное поле
        // Если у PlayerController есть публичное поле health
        return 100f; // Заглушка
    }
    
    float GetPlayerStamina(PlayerController playerController)
    {
        // Получаем стамину через рефлексию или публичное поле
        return 100f; // Заглушка
    }
    
    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Имя следующей сцены не указано!");
            return;
        }
        
        try
        {
            // Загружаем следующую сцену
            SceneManager.LoadScene(nextSceneName);
            Debug.Log($"Сцена {nextSceneName} загружается...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки сцены {nextSceneName}: {e.Message}");
            Debug.LogWarning("Убедитесь что сцена добавлена в Build Settings!");
        }
    }
    
    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = triggerColor;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
        
        // Стрелка показывающая направление
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);
        Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.2f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Более детальная визуализация при выделении
        Gizmos.color = new Color(triggerColor.r, triggerColor.g, triggerColor.b, 0.3f);
        Gizmos.DrawCube(transform.position, GetComponent<Collider>().bounds.size);
        
        // Показываем зону обнаружения
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // Подпись
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"→ {nextSceneName}", style);
        #endif
    }
    
    void OnDestroy()
    {
        // Восстанавливаем оригинальный материал
        if (triggerRenderer != null && originalMaterial != null)
        {
            triggerRenderer.material = originalMaterial;
        }
    }
}