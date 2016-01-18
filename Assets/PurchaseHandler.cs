using UnityEngine;
using System.Collections;
using sdkbox;

public class PurchaseHandler : MonoBehaviour 
{
	private sdkbox.IAP _iap;

	void Start() 
	{
		_iap = FindObjectOfType<sdkbox.IAP>();
		if (_iap == null)
		{
			Debug.Log("Failed to find IAP instance");
		}
	}

	public void Purchase() 
	{
		if (_iap != null)
		{
			Debug.Log("About to purchase coin_package");
			_iap.purchase("coin_package");
		}
	}

	public void Refresh() 
	{
		if (_iap != null)
		{
			Debug.Log("About to refresh");
			_iap.refresh();
		}
	}

	public void Restore() 
	{
		if (_iap != null)
		{
			Debug.Log("About to restore");
			_iap.restore();
		}
	}

	//
	// Event Handlers
	//

	public void onInitialized(bool status)
	{
		Debug.Log("PurchaseHandler.onInitialized " + status);
	}

	public void onSuccess(Product product)
	{
	}

	public void onFailure(Product product, string message)
	{
		Debug.Log("PurchaseHandler.onFailure " + message);
	}

	public void onCanceled(Product product)
	{
		Debug.Log("PurchaseHandler.onCanceled product: " + product.name);
	}

	public void onRestored()
	{
		Debug.Log("PurchaseHandler.onRestored");
	}

	public void onProductRequestSuccess(Product[] products)
	{
		foreach (var p in products)
		{
			Debug.Log("Product: " + p.name + " price: " + p.price);
		}
	}

	public void onProductRequestFailure(string message)
	{
	}

	public void onRestoreComplete(string message)
	{
	}
}
