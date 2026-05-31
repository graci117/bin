// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using NinjaTrader.Gui.Tools;
#endregion

namespace NinjaTrader.NinjaScript.ShareServices
{
	public class StockTwits : ShareService
	{
		private object icon;

		/// <summary>
		/// This MUST be overridden for any custom service properties to be copied over when instances of the service are created
		/// </summary>
		/// <param name="ninjaScript"></param>
		public override void CopyTo(NinjaScript ninjaScript)
		{
			base.CopyTo(ninjaScript);

			// Recompiling NinjaTrader.Custom after a Share service has been added will cause the Type to change.
			//  Use reflection to set the appropriate properties, rather than casting ninjaScript to StockTwits.
			PropertyInfo[] props = ninjaScript.GetType().GetProperties();
			foreach (PropertyInfo pi in props)
			{
				if (pi.Name == "OAuth_Token")
					pi.SetValue(ninjaScript, OAuthToken);
				else if (pi.Name == "StockTwitsSentiment")
					pi.SetValue(ninjaScript, StockTwitsSentiment);
				else if (pi.Name == "UserName")
					pi.SetValue(ninjaScript, UserName);
			}
		}

		public override object Icon => icon ??= Application.Current.TryFindResource("ShareIconStockTwits");

		public override Task OnAuthorizeAccount()
		{
			//Here we go through the OAuth 2.0 sign-in flow

			#region StockTwits Login Dialog
			const string oauthRequestTokenUrl	= "https://api.stocktwits.com/api/2/oauth/authorize";
			const string oauthConsumerKey		= "5cd7b6bdb6575757";
			const string oauthCallback			= "http://www.ninjatrader.com";

			//We're going to display a webpage in an NTWindow so the user can authorize our app to post on their behalf.
			//Because of WPF/WinForm airspace issues (see http://msdn.microsoft.com/en-us/library/aa970688.aspx for the gory details), 
			//	and because we want to have our pretty NT-styled windows, we need to finagle things a bit.
			//	1.) Create a modal NTWindow that will pop up when the user clicks "Connect"
			//	2.) Create a borderless window that will actually host the WebBrowser control
			//	3.) A window can have one Content object, so add a grid to the Window hosting the WebBrowser, and make the WeBrowser a child of the grid
			//	4.) Add another grid to the modal NTWindow. We'll use this to place where the WebBrowser goes
			//	5.) Handle the LocationChanged event for the NTWindow and the SizeChanged event for the placement grid. This will take care of making
			//		the hosted WebBrowser control look like it's part of the NTWindow
			//	6.) Make sure the Window hosting the WebBrowser is set to be TopMost so it appears on top of the NTWindow.
			NTWindow authWin = new()
			{
				Caption					= Custom.Resource.GuiAuthorize,
				IsModal					= true,
				Height					= 650,
				Width					= 900
			};

			Window webHost = new()
			{
				ResizeMode			= ResizeMode.NoResize,
				ShowInTaskbar		= false,
				WindowStyle			= WindowStyle.None
			};

			WebBrowser browser = new()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			Grid grid = new();
			grid.Children.Add(browser);
			webHost.Content = grid;

			Grid placementGrid = new();
			authWin.Content = placementGrid;
			
			authWin.LocationChanged		+= (_, _) => OnSizeLocationChanged(placementGrid, webHost);
			placementGrid.SizeChanged	+= (_, _) => OnSizeLocationChanged(placementGrid, webHost);

			string oauthToken = string.Empty;

			HideScriptErrors(browser);

			browser.Navigating += async (_, e) =>
			{
				if (e.Uri.Host == "www.ninjatrader.com")
				{
					if (e.Uri.Fragment.StartsWith("#access_token"))
					{
						//Successfully authorized! :D
						string		query = e.Uri.Fragment.TrimStart('#');
						string[]	pairs = query.Split('&');
						foreach (string pair in pairs)
						{
							string[] keyvalue = pair.Split('=');
							if (keyvalue[0] == "access_token")
								oauthToken = keyvalue[1];
						}

						OAuthToken				= oauthToken;

						// Verify the user's account so we can display the UserName
						string accountVerifyUri = $"https://api.stocktwits.com/api/2/account/verify.json?access_token={OAuthToken}";
						using (HttpClient client = new())
						{
							HttpResponseMessage				verifyResponse	= await client.GetAsync(accountVerifyUri);
							string							result			= await new StreamReader(verifyResponse.Content.ReadAsStreamAsync().Result).ReadToEndAsync();

							if (new JavaScriptSerializer().DeserializeObject(result) is not Dictionary<string, object> results)
							{
								LogAndPrint(typeof(Custom.Resource), "ShareStockTwitsNoAccount", null, Cbi.LogLevel.Error);
								authWin.DialogResult = false;
								authWin.Close();
								return;
							}

							if (results.TryGetValue("user", out object userinfo))
							{
								if (userinfo is Dictionary<string, object> user)
								{
									if (user.TryGetValue("username", out object username))
										UserName = username as string;
									else
									{
										LogAndPrint(typeof(Custom.Resource), "ShareStockTwitsNoAccount", null, Cbi.LogLevel.Error);
										authWin.DialogResult = false;
										authWin.Close();
										return;
									}
								}
								else
								{
									LogAndPrint(typeof(Custom.Resource), "ShareStockTwitsNoAccount", null, Cbi.LogLevel.Error);
									authWin.DialogResult = false;
									authWin.Close();
									return;
								}
							}
							else
							{
								LogAndPrint(typeof(Custom.Resource), "ShareStockTwitsNoAccount", null, Cbi.LogLevel.Error);
								authWin.DialogResult = false;
								authWin.Close();
								return;
							}
						}
						authWin.DialogResult	= true;
						authWin.Close();
					}
					else if (e.Uri.Fragment.StartsWith("#error"))
					{
						//User denied authorization :'(
						authWin.DialogResult = false;
						authWin.Close();
					}
				}
			};
			authWin.Closing += (_, _) => webHost.Close();

			string navigationUri = $"{oauthRequestTokenUrl}?client_id={oauthConsumerKey}&redirect_uri={oauthCallback}&response_type=token&scope=publish_messages";
			browser.Navigate(new Uri(navigationUri));
			webHost.Visibility	= Visibility.Visible;
			webHost.Topmost		= true;
			authWin.ShowDialog();

			if (authWin.DialogResult != true || string.IsNullOrEmpty(OAuthToken) || string.IsNullOrEmpty(UserName)) return Task.FromResult(0);

			#endregion

			IsConfigured = true;
			return Task.FromResult(0);
		}

		// The StockTwits auth page throws up a ton of IE scripting errors, so hide them.
		public static void HideScriptErrors(WebBrowser wb)
		{
			try
			{
				FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fiComWebBrowser == null)
					return;

				object objComWebBrowser = fiComWebBrowser.GetValue(wb);
				if (objComWebBrowser == null)
				{
					wb.Loaded += (_, _) => HideScriptErrors(wb); //In case we are too early
					return;
				}

				objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { true });
			}
			catch { }
		}

		public override async Task OnShare(string text, string imageFilePath)
		{
			string twit							= text.Normalize();
			string stocktwitsCreateMessageUrl	= $"https://api.stocktwits.com/api/2/messages/create.json?access_token={OAuthToken}";

			if (string.IsNullOrEmpty(imageFilePath))
			{
				using HttpClient client = new();
				string message = $"body={Core.Globals.UrlEncode(twit)}&sentiment={StockTwitsSentiment.ToString().ToLower()}";

				byte[]				encodedPostData		= new ASCIIEncoding().GetBytes(message);
				HttpContent			byteContent			= new ByteArrayContent(encodedPostData);
				HttpResponseMessage stockTwitsResponse	= await client.PostAsync(stocktwitsCreateMessageUrl, byteContent);
				string				result				= await new StreamReader(stockTwitsResponse.Content.ReadAsStreamAsync().Result).ReadToEndAsync();
				if (!stockTwitsResponse.IsSuccessStatusCode)
				{
					LogErrorResponse(result, stockTwitsResponse);
					return;
				}

				LogAndPrint(typeof(Custom.Resource), "ShareStockTwitsSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
			}
			else
			{
				if (!File.Exists(imageFilePath))
				{
					LogAndPrint(typeof(Custom.Resource), "ShareImageNoLongerExists", new object[] { imageFilePath }, Cbi.LogLevel.Error);
					SetState(State.Finalized);
					return;
				}

				using HttpClient client = new();
				using MultipartFormDataContent formData = new();
				string		url				= $"{stocktwitsCreateMessageUrl}&body={Core.Globals.UrlEncode(twit)}&sentiment={StockTwitsSentiment.ToString().ToLower()}";
				byte[]		imageBytes		= File.ReadAllBytes(imageFilePath);
				HttpContent imageContent	= new ByteArrayContent(imageBytes);
				imageContent.Headers.Add("Content-Type", "image/png");
				formData.Add(imageContent, "chart", "photo.png");	//Be sure to include a file name

				HttpResponseMessage stockTwitsResponse	= await client.PostAsync(url, formData);
				string				result				= await new StreamReader(stockTwitsResponse.Content.ReadAsStreamAsync().Result).ReadToEndAsync();

				if (!stockTwitsResponse.IsSuccessStatusCode)
				{
					LogErrorResponse(result, stockTwitsResponse);
					return;
				}

				LogAndPrint(typeof(Custom.Resource), "ShareStockTwitsSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
			}
		}

		public override async Task OnShare(string text, string imageFilePath, object[] args)
		{
			if (args is { Length: > 0 })
			{
				try
				{
					Sentiment sentiment = (Sentiment)args[0];
					StockTwitsSentiment = sentiment;
				}
				catch (Exception exp)
				{
					LogAndPrint(typeof(Custom.Resource), "ShareArgsException", new object[] { exp.Message }, Cbi.LogLevel.Error);
					return;
				}
			}

			await OnShare(text, imageFilePath);
		}

		private void LogErrorResponse(string result, HttpResponseMessage stockTwitsResponse)
		{
			switch (stockTwitsResponse.StatusCode)
			{
				case HttpStatusCode.BadRequest:
					// If the request is invalid or can't be served, or a request is sent without authorization, you'll get a 400 response
					LogAndPrint(typeof(Custom.Resource), "ShareBadRequestError", new object[] { result }, Cbi.LogLevel.Error);
					break;
				case HttpStatusCode.Unauthorized:
					//If stocktwits can't or won't authenticate the request, you'll get a 401 response
					LogAndPrint(typeof(Custom.Resource), "ShareNotAuthorized", new object[] { result }, Cbi.LogLevel.Error);
					break;
				case HttpStatusCode.Forbidden:
					LogAndPrint(typeof(Custom.Resource), "ShareForbidden", new object[] { result }, Cbi.LogLevel.Error);
					break;
				case (HttpStatusCode)429:
					// If you've requested a resource too many times in a short amount of time, you'll get a 429 response
					LogAndPrint(typeof(Custom.Resource), "ShareTooManyRequests", new object[] { result }, Cbi.LogLevel.Error);
					break;
				case HttpStatusCode.InternalServerError:
					// If something breaks on Twitter's side, you'll get a 500 response
					LogAndPrint(typeof(Custom.Resource), "ShareInternalServerError", new object[] { result }, Cbi.LogLevel.Error);
					break;
				case HttpStatusCode.ServiceUnavailable:
					// If Twitter's servers are up but overloaded with requests, you'll get a 503 error
					LogAndPrint(typeof(Custom.Resource), "ShareBadGatewayError", new object[] { result }, Cbi.LogLevel.Error);
					break;
				case HttpStatusCode.GatewayTimeout:
					// If Twitter's servers are up but the request couldn't be serviced because of some problem on their end, you'll get a 504 response
					LogAndPrint(typeof(Custom.Resource), "ShareGatewayTimeoutError", new object[] { result }, Cbi.LogLevel.Error);
					break;
				default:
					LogAndPrint(typeof(Custom.Resource), "ShareNonSuccessCode", new object[] { stockTwitsResponse.StatusCode, result }, Cbi.LogLevel.Error);
					break;
			}
		}

		private static void OnSizeLocationChanged(FrameworkElement placementTarget, Window webHost)
		{
			//Here we set the location and size of the borderless Window hosting the WebBrowser control. 
			//	This is based on the location and size of the child grid of the NTWindow. When the grid changes,
			//	the hosted WebBrowser changes to match.
			if (webHost.Visibility == Visibility.Visible)
				webHost.Show();

			webHost.Owner							= Window.GetWindow(placementTarget);
			Point				locationFromScreen	= placementTarget.PointToScreen(new(0, 0));
			PresentationSource	source				= PresentationSource.FromVisual(webHost);
			if (source is { CompositionTarget: not null })
			{
				Point targetPoints	= source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
				webHost.Left		= targetPoints.X;
				webHost.Top			= targetPoints.Y;
			}

			webHost.Width	= placementTarget.ActualWidth;
			webHost.Height	= placementTarget.ActualHeight;
		}

		protected override void OnStateChange()
		{			
			if (State == State.SetDefaults)
			{
				CharacterLimit				= 1000;
				IsConfigured				= false;
				IsDefault					= false;
				IsImageAttachmentSupported	= true;
				Name						= Custom.Resource.StockTwitsServiceName;
				Signature					= string.Empty;
				UseOAuth					= true;
				UserName					= string.Empty;

				StockTwitsSentiment			= Sentiment.Neutral;
			}
			else if (State == State.Active)
				CharactersReservedPerMedia = 40;
			else if (State == State.Terminated) StockTwitsSentiment = Sentiment.Neutral;
		}

		#region Properties
		[Gui.Encrypt]
		[Browsable(false)]
		public string OAuthToken { get; set; }

		[Browsable(false)]
		[ShareField]																													   //This indicates this property should show up in the Share window
		[Display(ResourceType = typeof(Custom.Resource), Name = "StockTwitsSentiment", Description = "StockTwitsSentimentDescription")]	   //The name will show up in the text label on the Share window and the Description will be the tooltip
		public Sentiment StockTwitsSentiment { get; set; }

		[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShareServiceUserName", GroupName = "ShareServiceParameters", Order = 1)]
		public string UserName { get; set; }

		public enum Sentiment
		{
			Neutral,
			Bearish,
			Bullish
		}
		#endregion
	}
}
