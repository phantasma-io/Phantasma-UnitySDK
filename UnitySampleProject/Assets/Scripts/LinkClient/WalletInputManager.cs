using UnityEngine;
using TMPro;

public class WalletInputManager : MonoBehaviour
{
    public TMP_InputField recipientInput;
    public TMP_InputField symbolInput;
    public TMP_InputField decimalsInput;
    public TMP_InputField amountInput;
    public TMP_InputField payloadInput; // optional
    public TMP_InputField chainInput; // optional
    public TMP_InputField tokenIdInput; // unused atm
    public TMP_InputField dataToSignInput;

    public void ApplyInputs()
    {
        WalletInteractions.RecipientAddress = recipientInput.text;
        WalletInteractions.Symbol = symbolInput.text;
        WalletInteractions.Decimals = int.TryParse(decimalsInput.text, out var valueDecimals) ? valueDecimals : null;
        WalletInteractions.Amount = decimal.TryParse(amountInput.text, out var valueAmount) ? valueAmount : 0;
        WalletInteractions.Payload = payloadInput.text;
        WalletInteractions.Chain = string.IsNullOrWhiteSpace(chainInput.text) ? "main" : chainInput.text;
        WalletInteractions.TokenId = tokenIdInput.text;
        WalletInteractions.DataToSign = dataToSignInput.text;

        Debug.Log($"[Input] To: {WalletInteractions.RecipientAddress}, Symbol: {WalletInteractions.Symbol}, Decimals: {WalletInteractions.Decimals?.ToString() ?? ""}, Amount: {WalletInteractions.Amount}, Payload: {WalletInteractions.Payload ?? ""}, Chain: {WalletInteractions.Chain}, TokenId: {WalletInteractions.TokenId ?? ""}, DataToSign: {WalletInteractions.DataToSign ?? ""}");
    }
}
