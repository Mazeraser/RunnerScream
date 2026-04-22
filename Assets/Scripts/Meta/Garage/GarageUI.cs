using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Meta;

namespace Garage
{
    public class GarageUI : MonoBehaviour
    {
        [Header("Main UI")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject garagePanel;
        [SerializeField] private TextMeshProUGUI carNameText;
        [SerializeField] private TextMeshProUGUI carDescriptionText;
        
        [Header("Car Stats")]
        [SerializeField] private TextMeshProUGUI speedValueText;
        [SerializeField] private TextMeshProUGUI accelerationValueText;
        [SerializeField] private TextMeshProUGUI healthValueText;
        
        [Header("Buttons")]
        [SerializeField] private Button purchaseButton;   // кнопка "Купить"
        [SerializeField] private Button selectButton;     // кнопка "Выбрать"
        [SerializeField] private Button selectedButton;   // кнопка "Выбрано" (просто индикатор)
        [SerializeField] private TextMeshProUGUI priceText;
        
        [Header("Navigation")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        
        private void Start()
        {
            if (GarageManager.Instance == null)
            {
                Debug.LogError("GarageManager не найден!");
                return;
            }
            
            // Подписываемся на события менеджера для автоматического обновления UI
            GarageManager.Instance.OnCarChanged += OnCarChangedHandler;
            GarageManager.Instance.OnCarPurchased += OnCarPurchasedHandler;
            
            // Назначаем обработчики кнопок
            if (prevButton != null)
                prevButton.onClick.AddListener(() => GarageManager.Instance.PreviousCar());
            if (nextButton != null)
                nextButton.onClick.AddListener(() => GarageManager.Instance.NextCar());
            if (selectButton != null)
                selectButton.onClick.AddListener(() => GarageManager.Instance.SelectCurrentCar());
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(() => GarageManager.Instance.PurchaseCurrentCar());
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseGarage);
            
            // Изначально кнопка "Выбрано" неинтерактивна (просто индикатор)
            if (selectedButton != null)
                selectedButton.interactable = false;
            
            garagePanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий при уничтожении
            if (GarageManager.Instance != null)
            {
                GarageManager.Instance.OnCarChanged -= OnCarChangedHandler;
                GarageManager.Instance.OnCarPurchased -= OnCarPurchasedHandler;
            }
        }
        
        private void OnCarChangedHandler(CarDataSO carData)
        {
            // При переключении машины обновляем UI
            UpdateUI(carData, IsCarUnlocked(carData), IsCarSelected(carData));
        }
        
        private void OnCarPurchasedHandler(CarDataSO carData)
        {
            // После покупки обновляем валюту и текущий UI
            if (MenuCurrencyUtils.Instance != null)
                MenuCurrencyUtils.Instance.RefreshCurrencyDisplay();
            
            UpdateUI(carData, true, IsCarSelected(carData));
        }
        
        public void OpenGarage()
        {
            garagePanel.SetActive(true);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            
            if (MenuCurrencyUtils.Instance != null)
                MenuCurrencyUtils.Instance.RefreshCurrencyDisplay();
            
            if (GarageManager.Instance != null)
                GarageManager.Instance.RefreshGarage();
        }
        
        public void CloseGarage()
        {
            garagePanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }
        
        public void UpdateUI(CarDataSO carData, bool isUnlocked, bool isSelected)
        {
            if (carData == null) return;
            
            // Основная информация
            if (carNameText != null) carNameText.text = carData.carName;
            if (carDescriptionText != null) carDescriptionText.text = carData.description;
            
            UpdateStats(carData);
            UpdateButtonsState(isUnlocked, isSelected);
            UpdatePriceText(carData, isUnlocked);
            UpdateNavigationButtons();
        }
        
        private void UpdateStats(CarDataSO carData)
        {
            if (carData == null) return;
            
            if (speedValueText != null)
                speedValueText.text = $"{carData.baseMaxSpeed:F0}";
            if (accelerationValueText != null)
                accelerationValueText.text = $"{carData.baseAcceleration:F1}";
            if (healthValueText != null)
                healthValueText.text = $"{carData.baseHealth}";
        }
        
        private void UpdateButtonsState(bool isUnlocked, bool isSelected)
        {
            // Кнопка "Купить" активна только если машина НЕ разблокирована
            if (purchaseButton != null)
                purchaseButton.interactable = !isUnlocked;
            
            // Кнопка "Выбрать" активна только если машина разблокирована И НЕ выбрана
            if (selectButton != null)
                selectButton.interactable = isUnlocked && !isSelected;
            
            // Кнопка "Выбрано" — просто индикатор, её интерактивность всегда false
            // (можно также менять цвет текста или спрайт, но она не должна быть кликабельной)
            if (selectedButton != null)
            {
                selectedButton.interactable = false;
                // Дополнительно можно менять текст или цвет, чтобы показать статус
                var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = isSelected ? "ВЫБРАНО" : "ВЫБРАНО";
                // Можно изменить цвет, если выбрано
                // text.color = isSelected ? Color.green : Color.gray;
            }
        }
        
        private void UpdatePriceText(CarDataSO carData, bool isUnlocked)
        {
            if (priceText == null) return;
            
            if (isUnlocked)
            {
                priceText.text = "Куплена";
                return;
            }
            
            if (carData.priceSoft <= 0 && carData.priceHard <= 0)
                priceText.text = "Бесплатно";
            else if (carData.priceSoft > 0 && carData.priceHard > 0)
                priceText.text = $"{carData.priceSoft} / {carData.priceHard}";
            else if (carData.priceSoft > 0)
                priceText.text = $"{carData.priceSoft} монет";
            else
                priceText.text = $"{carData.priceHard} кристаллов";
        }
        
        private void UpdateNavigationButtons()
        {
            if (GarageManager.Instance == null) return;
            
            // Используем GetAllCars() вместо GetAvailableCars(), чтобы навигация работала по всем машинам
            var allCars = GarageManager.Instance.GetAllCars();
            bool hasMultiple = allCars.Count > 1;
            
            if (prevButton != null) prevButton.interactable = hasMultiple;
            if (nextButton != null) nextButton.interactable = hasMultiple;
        }
        
        // Вспомогательные методы для получения статуса машины (чтобы не обращаться к UserData напрямую в обработчиках)
        private bool IsCarUnlocked(CarDataSO carData)
        {
            var data = DataBase.UserData.GetCarData(carData.carId);
            return data.isUnlocked;
        }
        
        private bool IsCarSelected(CarDataSO carData)
        {
            return DataBase.UserData.SelectedCar == carData.carId;
        }
    }
}