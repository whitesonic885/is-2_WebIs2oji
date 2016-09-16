using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace is2oji
{
	/// <summary>
	/// [is2oji]
	/// </summary>
	//--------------------------------------------------------------------------
	// C³—š—ğ
	//--------------------------------------------------------------------------
	// 2010.12.14 ACTjŠ_Œ´ V‹Kì¬
	//--------------------------------------------------------------------------
	// MOD 2011.03.09 “Œ“sj‚–Ø ¿‹æƒ}ƒXƒ^‚ÌåƒL[‚É[‰ïˆõ‚b‚c]‚ğ’Ç‰Á 
	//--------------------------------------------------------------------------
	// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü 
	// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ 
	// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ 
	// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰
	// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä 
	// MOD 2011.06.01 “Œ“sj‚–Ø ‚r‚p‚k‚Ì’²® 
	// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á 
	// MOD 2011.10.06 “Œ“sj‚–Ø o‰×ƒf[ƒ^‚ÌˆóüƒƒO‚Ì’Ç‰Á 
	// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš 
	//--------------------------------------------------------------------------
	// MOD 2015.05.01 BEVAS) ‘O“c CM14J—X•Ö”Ô†‘¶İƒ`ƒFƒbƒN
	//--------------------------------------------------------------------------
	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2oji")]

	public class Service1 : is2common.CommService
	{
		private static string sKanma = ",";
		private static string sDbl = "\"";
		private static string sSng = "'";

		public Service1()
		{
			//CODEGEN: ‚±‚ÌŒÄ‚Ño‚µ‚ÍAASP.NET Web ƒT[ƒrƒX ƒfƒUƒCƒi‚Å•K—v‚Å‚·B
			InitializeComponent();

			connectService();
		}

		#region ƒRƒ“ƒ|[ƒlƒ“ƒg ƒfƒUƒCƒi‚Å¶¬‚³‚ê‚½ƒR[ƒh 
		
		//Web ƒT[ƒrƒX ƒfƒUƒCƒi‚Å•K—v‚Å‚·B
		private IContainer components = null;
				
		/// <summary>
		/// ƒfƒUƒCƒi ƒTƒ|[ƒg‚É•K—v‚Èƒƒ\ƒbƒh‚Å‚·B‚±‚Ìƒƒ\ƒbƒh‚Ì“à—e‚ğ
		/// ƒR[ƒh ƒGƒfƒBƒ^‚Å•ÏX‚µ‚È‚¢‚Å‚­‚¾‚³‚¢B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// g—p‚³‚ê‚Ä‚¢‚éƒŠƒ\[ƒX‚ÉŒãˆ—‚ğÀs‚µ‚Ü‚·B
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/*********************************************************************
		 * ”­“Xæ“¾
		 * ˆø”F‰×‘—l‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA“XŠ‚b‚cA“XŠ–¼A“s“¹•{Œ§‚b‚cAs‹æ’¬‘º‚b‚cA‘åš’ÊÌ‚b‚c
		 *
		 *********************************************************************/
		private static string GET_HATUTEN3_SELECT
			= "SELECT CM14.“XŠ‚b‚c \n"
			+  " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
			+      ", ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
			;

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2init\Service1.asmx.cs(3062):
		*/
		[WebMethod]
		public String[] Get_hatuten3(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "”­“Xæ“¾‚RŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_HATUTEN3_SELECT);
				sbQuery.Append(" WHERE CM02.‰ïˆõ‚b‚c = '" + sKcode + "' \n");
				sbQuery.Append(" AND CM02.•”–å‚b‚c = '" + sBcode + "' \n");
				sbQuery.Append(" AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				if(reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();

					if (sRet[1].Equals("999")) // ‰¤q‰^‘—‘Î‰
					{
						sRet[0] = "w’è‚µ‚½ZŠ‚ÍA”z’B•s‰Â”\ƒGƒŠƒA‚Å‚·";
					}
					else
					{
						sRet[0] = "³íI—¹";
					}
				}
				else
				{
					sRet[0] = "—˜—pÒ‚ÌW‰×“Xæ“¾‚É¸”s‚µ‚Ü‚µ‚½";
				}
				disposeReader(reader);
				reader = null;

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * •”–åƒ}ƒXƒ^ŒŸõ
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA•”–å‚b‚cA•”–å–¼Ao—Í‡A“XŠ–¼AXV“ú
		 *
		 * QÆŒ³F‰ïˆõƒ}ƒXƒ^.cs 2‰ñ
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(717):
		*/
		[WebMethod]
		public string[] Sel_Section(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "•”–åƒ}ƒXƒ^ŒŸõŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[19];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT CM02.•”–å‚b‚c "
					+      ", CM02.•”–å–¼ "
					+      ", CM02.o—Í‡ "
					+      ", CM02.—X•Ö”Ô† "
					+      ", NVL(CM10.“XŠ–¼, ' ') "
					+      ", CM02.İ’uæZŠ‚P "
					+      ", CM02.İ’uæZŠ‚Q "
					+      ", CM02.XV“ú \n"
					+      ", CM02.ƒT[ƒ}ƒ‹‘ä” \n"
					+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚P,' ') \n"
					+      ", NVL(CM06.ó‘Ô‚P,' ') \n"
					+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚Q,' ') \n"
					+      ", NVL(CM06.ó‘Ô‚Q,' ') \n"
					+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚R,' ') \n"
					+      ", NVL(CM06.ó‘Ô‚R,' ') \n"
					+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚S,' ') \n"
					+      ", NVL(CM06.ó‘Ô‚S,' ') \n"
					+      ", NVL(CM06.g—p—¿,0) \n"
					+  " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
					+      " LEFT JOIN ‚b‚l‚O‚U•”–åŠg’£ CM06 \n"
					+      " ON CM02.‰ïˆõ‚b‚c = CM06.‰ïˆõ‚b‚c \n"
					+      " AND CM02.•”–å‚b‚c = CM06.•”–å‚b‚c \n"
					+  " LEFT JOIN ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
					+    " ON CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† "
					+  " LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
					+    " ON CM14.“XŠ‚b‚c = CM10.“XŠ‚b‚c "
					+   " AND CM10.íœ‚e‚f = '0' \n"
					+ " WHERE CM02.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
					+   " AND CM02.•”–å‚b‚c = '" + sKey[1] + "' \n"
					+   " AND CM02.íœ‚e‚f = '0' \n"
					;

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				while (reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetDecimal(2).ToString().Trim();
					sRet[4] = reader.GetString(3).Trim();
					sRet[5] = reader.GetString(4).Trim();
					sRet[6] = reader.GetString(5).Trim();
					sRet[7] = reader.GetString(6).Trim();
					sRet[8] = reader.GetDecimal(7).ToString().Trim();
					sRet[9] = reader.GetDecimal(8).ToString().Trim();
					sRet[10] = reader.GetString(9).Trim();
					sRet[11] = reader.GetString(10).Trim();
					sRet[12] = reader.GetString(11).Trim();
					sRet[13] = reader.GetString(12).Trim();
					sRet[14] = reader.GetString(13).Trim();
					sRet[15] = reader.GetString(14).Trim();
					sRet[16] = reader.GetString(15).Trim();
					sRet[17] = reader.GetString(16).Trim();
					sRet[18] = reader.GetDecimal(17).ToString().Trim();
					iCnt++;
				}
				if(sRet[11].Trim().Length == 0) sRet[11] = "0"; 
				if(sRet[13].Trim().Length == 0) sRet[13] = "0"; 
				if(sRet[15].Trim().Length == 0) sRet[15] = "0"; 
				if(sRet[17].Trim().Length == 0) sRet[17] = "0"; 
				if(sRet[18].Trim().Length == 0) sRet[18] = "0"; 
				disposeReader(reader);
				reader = null;
				if(iCnt == 1) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
					sRet[0] = "³íI—¹";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ¿‹æƒ}ƒXƒ^ˆê——æ“¾
		 * ˆø”F“XŠ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAˆê——i—X•Ö”Ô†A“¾ˆÓæ‚b‚cj...
		 *
		 * QÆŒ³F¿‹æƒ}ƒXƒ^.cs Œ»İ–¢g—p
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(2797):
		*/
		[WebMethod]
		public string[] Get_Claim(string[] sUser, string sKey)
		{
			logWriter(sUser, INF, "¿‹æƒ}ƒXƒ^ˆê——æ“¾ŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(SM04.—X•Ö”Ô†) || '|' "
					+     "|| TRIM(SM04.“¾ˆÓæ‚b‚c)     || '|' "
					+     "|| TRIM(SM04.“¾ˆÓæ•”‰Û‚b‚c) || '|' "
					+     "|| TRIM(SM04.“¾ˆÓæ•”‰Û–¼)   || '|' "
					+     "|| TRIM(SM04.‰ïˆõ‚b‚c) || '|' "
					+     "|| NVL(CM01.‰ïˆõ–¼, ' ')  || '|' "
					+     "|| TO_CHAR(SM04.XV“ú) || '|' \n"
					+  " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 " // ‰¤q‰^‘—‘Î‰
					+      ", ‚r‚l‚O‚S¿‹æ SM04 \n"
					+  " LEFT JOIN ‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+    " ON SM04.‰ïˆõ‚b‚c = CM01.‰ïˆõ‚b‚c "
					+    "AND '0' = CM01.íœ‚e‚f \n"
					+ " WHERE CM14.“XŠ‚b‚c = '" + sKey + "' \n"
					+   " AND CM14.—X•Ö”Ô† = SM04.—X•Ö”Ô† \n"
					+   " AND SM04.íœ‚e‚f = '0' \n"
					+ " ORDER BY SM04.‰ïˆõ‚b‚c "
					+          ",SM04.“¾ˆÓæ‚b‚c "
					+          ",SM04.“¾ˆÓæ•”‰Û‚b‚c \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ¿‹æƒ}ƒXƒ^ˆê——æ“¾‚Q
		 * ˆø”F“XŠ‚b‚cA‰ïˆõ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAˆê——i—X•Ö”Ô†A“¾ˆÓæ‚b‚cj...
		 *
		 * QÆŒ³F¿‹æƒ}ƒXƒ^.cs
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(2908):
		*/
		[WebMethod]
		public string[] Get_Claim2(string[] sUser, string sTensyo, string sKaiin)
		{
			logWriter(sUser, INF, "¿‹æƒ}ƒXƒ^ˆê——æ“¾‚QŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(SM04.—X•Ö”Ô†) || '|' "
					+     "|| TRIM(SM04.‰ïˆõ‚b‚c) || '|' "
					+     "|| NVL(TRIM(CM01.‰ïˆõ–¼), ' ')  || '|' "
					+     "|| TRIM(SM04.“¾ˆÓæ‚b‚c)     || '|' "
					+     "|| TRIM(SM04.“¾ˆÓæ•”‰Û‚b‚c) || '|' "
					+     "|| TRIM(SM04.“¾ˆÓæ•”‰Û–¼)   || '|' "
					+     "|| TO_CHAR(SM04.XV“ú) || '|' \n"
					+  " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 " // ‰¤q‰^‘—‘Î‰
					+      ", ‚r‚l‚O‚S¿‹æ SM04 \n"
					+  " LEFT JOIN ‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+    " ON SM04.‰ïˆõ‚b‚c = CM01.‰ïˆõ‚b‚c "
					+    "AND '0' = CM01.íœ‚e‚f \n"
					+ " WHERE CM14.“XŠ‚b‚c = '" + sTensyo + "' \n";

				if(sKaiin.Length > 0)
				{
					cmdQuery += "AND  SM04.‰ïˆõ‚b‚c = '" + sKaiin + "' \n";
				}
				cmdQuery
					+=  " AND CM14.—X•Ö”Ô† = SM04.—X•Ö”Ô† \n"
					+   " AND SM04.íœ‚e‚f = '0' \n"
					+   " AND CM01.ŠÇ—Ò‹æ•ª IN ('1','3','4') \n" // 1:ŠÇ—Ò 3:‰¤qˆê”Ê 4:‰¤q‰c‹ÆŠ
					+ " ORDER BY SM04.‰ïˆõ‚b‚c "
					+          ",SM04.“¾ˆÓæ‚b‚c "
					+          ",SM04.“¾ˆÓæ•”‰Û‚b‚c \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;

		}

		/*********************************************************************
		 * —X•Ö”Ô†ƒ}ƒXƒ^æ“¾
		 * ˆø”F—X•Ö”Ô†
		 * –ß’lFƒXƒe[ƒ^ƒXA“XŠ–¼
		 *
		 * QÆŒ³F‰ïˆõƒ}ƒXƒ^.cs
		 * QÆŒ³F¿‹æƒ}ƒXƒ^.cs
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(3668):
		*/
		[WebMethod]
		public string[] Sel_Postcode(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "—X•Ö”Ô†ƒ}ƒXƒ^ŒŸõŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[4]{"","","",""};

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT NVL(CM10.“XŠ–¼, ' '), \n"
					+ " TRIM(CM14.“s“¹•{Œ§–¼) || TRIM(CM14.s‹æ’¬‘º–¼) || TRIM(CM14.’¬ˆæ–¼) \n"
					+ ", CM14.“XŠ‚b‚c \n"
					+  " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
					+  " LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
					+    " ON CM14.“XŠ‚b‚c = CM10.“XŠ‚b‚c "
					+    "AND CM10.íœ‚e‚f = '0' \n"
					+ " WHERE CM14.—X•Ö”Ô† = '" + sKey[0] + "' \n"
					;

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				while (reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim();
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if(iCnt == 1)
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				else
				{
					if (sRet[3].Equals("999")) // ‰¤q‰^‘—‘Î‰
					{
						sRet[0] = "w’è‚µ‚½ZŠ‚ÍA”z’B•s‰Â”\ƒGƒŠƒA‚Å‚·";
					}
					else
					{
						sRet[0] = "³íI—¹";
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * —X•Ö”Ô†ƒ}ƒXƒ^æ“¾
		 * ˆø”F—X•Ö”Ô†
		 * –ß’lFƒXƒe[ƒ^ƒXAZŠA“XŠ³®–¼A“XŠ‚b‚c
		 *
		 * QÆŒ³F‰ïˆõ‰Á“ü.cs		[]	
		 * QÆŒ³F“XŠî•ñ.cs		[]	
		 * QÆŒ³F¿‹æƒ}ƒXƒ^.cs	[]	
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(7318):
		*/
		[WebMethod]
		public string[] Sel_Postcode1(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "—X•Ö”Ô†ƒ}ƒXƒ^ŒŸõŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT NVL(CM10.“XŠ–¼, ' '), \n"
					+ " TRIM(CM14.“s“¹•{Œ§–¼) || TRIM(CM14.s‹æ’¬‘º–¼) || TRIM(CM14.’¬ˆæ–¼),NVL(TRIM(CM10.“XŠ³®–¼), ' '),TRIM(CM14.“XŠ‚b‚c) \n"
					+  " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
					+  " LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
					+    " ON CM14.“XŠ‚b‚c = CM10.“XŠ‚b‚c "
					+    "AND CM10.íœ‚e‚f = '0' \n"
					+ " WHERE CM14.—X•Ö”Ô† = '" + sKey[0] + "' \n"
					;

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				while (reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim();
					sRet[4] = reader.GetString(3).Trim();
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if(iCnt == 1) 
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				else
				{
					if (sRet[4].Equals("999")) // ‰¤q‰^‘—‘Î‰
					{
						sRet[0] = "w’è‚µ‚½ZŠ‚ÍA”z’B•s‰Â”\ƒGƒŠƒA‚Å‚·";
					}
					else
					{
						sRet[0] = "³íI—¹";
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ƒƒOƒCƒ“”FØ
		 * ˆø”F‰ïˆõ‚b‚cA—˜—pÒ‚b‚cAƒpƒXƒ[ƒh
		 * –ß’lFƒXƒe[ƒ^ƒXA‰ïˆõ‚b‚cA‰ïˆõ–¼A—˜—pÒ‚b‚cA—˜—pÒ–¼
		 *
		 *********************************************************************/
		private static string SET_LOGIN_SELECT1
			= "SELECT CM01.‰ïˆõ‚b‚c, \n"
			+ " CM01.‰ïˆõ–¼, \n"
			+ " CM04.—˜—pÒ‚b‚c, \n"
			+ " CM04.—˜—pÒ–¼ \n"
			+ ", CM01.ŠÇ—Ò‹æ•ª \n"
			+ ", NVL(CM14.“XŠ‚b‚c,' ') \n"
			+ " FROM ‚b‚l‚O‚P‰ïˆõ CM01, \n"
			+ " ‚b‚l‚O‚Q•”–å CM02, \n"
			+ " ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14, \n" // ‰¤q‰^‘—‘Î‰
			+ " ‚b‚l‚O‚S—˜—pÒ CM04 \n";

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(4657):
		*/
		[WebMethod]
		public string[] Set_login(string[] sUser, string[] sKey) 
		{
			logWriter(sUser, INF, "ƒƒOƒCƒ“”FØŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[7];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= SET_LOGIN_SELECT1
					+ " WHERE CM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
					+   " AND CM01.‰ïˆõ‚b‚c = CM04.‰ïˆõ‚b‚c \n"
					+   " AND CM04.—˜—pÒ‚b‚c = '" + sKey[1] + "' \n"
					+   " AND CM04.ƒpƒXƒ[ƒh = '" + sKey[2] + "' \n"
					+   " AND CM01.g—pŠJn“ú <= TO_CHAR(SYSDATE,'YYYYMMDD') \n"
					+   " AND CM01.g—pI—¹“ú >= TO_CHAR(SYSDATE,'YYYYMMDD') \n"
					+   " AND CM01.ŠÇ—Ò‹æ•ª IN ('1','4') \n" // 1:ŠÇ—Ò 4:‰¤q‰c‹ÆŠ
					+   " AND CM01.íœ‚e‚f = '0' \n"
					+   " AND CM04.íœ‚e‚f = '0' \n"
					+   " AND CM04.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
					+   " AND CM04.•”–å‚b‚c = CM02.•”–å‚b‚c \n"
					+   " AND           '0' = CM02.íœ‚e‚f \n"
					+   " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô†(+) \n"
					;

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if (reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim();
					sRet[4] = reader.GetString(3).Trim();
					sRet[5] = reader.GetString(4).Trim();
					sRet[6] = reader.GetString(5).Trim();
					if (sRet[6].Equals("999")) // ‰¤q‰^‘—‘Î‰
					{
						sRet[0] = "w’è‚µ‚½ZŠ‚ÍA”z’B•s‰Â”\ƒGƒŠƒA‚Å‚·";
					}
					else
					{
						sRet[0] = "³íI—¹";
					}
				}
				else
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				disposeReader(reader);
				reader = null;
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ‰ïˆõî•ñæ“¾i‚b‚r‚uo—Í—pj
		 * ˆø”F‰ïˆõ‚b‚cAg—pŠJn“úiŠJnAI—¹jAg—pI—¹“úiŠJnAI—¹jA
		 *		 —˜—pÒ“o˜^“úiŠJnAI—¹j
		 * –ß’lFƒXƒe[ƒ^ƒXA‰ïˆõ‚b‚cA‰ïˆõ–¼Ag—pŠJn“ú...
		 *
		 * QÆŒ³F‰ïˆõî•ñ‚b‚r‚uo—Í.cs
		 *********************************************************************/
		private static string GET_KAIINCSV_SELECT
			= "SELECT R.‰ïˆõ‚b‚c,NVL(K.‰ïˆõ–¼,' '),NVL(K.g—pŠJn“ú,' '),NVL(K.g—pI—¹“ú,' '), \n"
			+       " R.•”–å‚b‚c,NVL(B.•”–å–¼,' '),NVL(Y.“XŠ‚b‚c,' '),NVL(T.“XŠ–¼,' '), \n"
			+       " NVL(B.İ’uæZŠ‚P,' '),NVL(B.İ’uæZŠ‚Q,' '), \n"
			+       " R.—˜—pÒ‚b‚c,R.\"ƒpƒXƒ[ƒh\",R.—˜—pÒ–¼,SUBSTR(R.“o˜^“ú,1,8) \n"
			+       " ,NVL(B.\"ƒT[ƒ}ƒ‹‘ä”\",'0')\n"
			+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚P,' '), DECODE(CM06.ó‘Ô‚P,'1 ','•Ô•i','2 ','•s—Ç•i','3 ','•s–¾','4 ','‚»‚Ì‘¼','5 ','”­‘—’†',' ') \n"
			+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚Q,' '), DECODE(CM06.ó‘Ô‚Q,'1 ','•Ô•i','2 ','•s—Ç•i','3 ','•s–¾','4 ','‚»‚Ì‘¼','5 ','”­‘—’†',' ') \n"
			+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚R,' '), DECODE(CM06.ó‘Ô‚R,'1 ','•Ô•i','2 ','•s—Ç•i','3 ','•s–¾','4 ','‚»‚Ì‘¼','5 ','”­‘—’†',' ') \n"
			+      ", NVL(CM06.ƒVƒŠƒAƒ‹”Ô†‚S,' '), DECODE(CM06.ó‘Ô‚S,'1 ','•Ô•i','2 ','•s—Ç•i','3 ','•s–¾','4 ','‚»‚Ì‘¼','5 ','”­‘—’†',' ') \n"
			+      ", DECODE(K.ŠÇ—Ò‹æ•ª,'1','ŠÇ—Ò','3','‰¤qˆê”Ê','4','‰¤q‰c‹ÆŠ', K.ŠÇ—Ò‹æ•ª) \n"
			+      ", DECODE(K.‹L–˜AŒg‚e‚f,'0',' ','1','‰^’À”ñ•\¦', K.‹L–˜AŒg‚e‚f) \n"
			+      ", K.“o˜^“ú, K.XV“ú \n"
			+      ", B.‘gD‚b‚c, B.—X•Ö”Ô†, NVL(CM06.g—p—¿,0) \n"
			+      ", DECODE(CM06.‰ïˆõ\ŠÇ—”Ô†,NULL,' ',0,' ',TO_CHAR(CM06.‰ïˆõ\ŠÇ—”Ô†)) \n"
			+      ", B.“o˜^“ú, B.XV“ú \n"
			+      ", R.‰×‘—l‚b‚c \n"
			+      ", DECODE(R.Œ ŒÀ‚P,' ',' ','1','ƒ‰ƒxƒ‹ˆóü‹Ö~', R.Œ ŒÀ‚P) \n"
			+      ", R.\"”FØƒGƒ‰[‰ñ”\" \n"
			+      ", R.“o˜^‚o‚f \n"
			+      ", R.“o˜^“ú, R.XV“ú \n"
			+ " FROM ‚b‚l‚O‚P‰ïˆõ K,‚b‚l‚O‚Q•”–å B,‚b‚l‚O‚S—˜—pÒ R,‚b‚l‚P‚O“XŠ T,‚b‚l‚P‚S—X•Ö”Ô†‚i Y \n" // ‰¤q‰^‘—‘Î‰
			+ " ,‚b‚l‚O‚U•”–åŠg’£ CM06 \n"
			;

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(5304):
		*/
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "‰ïˆõî•ñ‚b‚r‚uo—Í—pæ“¾ŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbQuery2 = new StringBuilder(1024);
			try
			{
				sbQuery.Append(" WHERE R.‰ïˆõ‚b‚c = K.‰ïˆõ‚b‚c \n");
				sbQuery.Append(" AND R.‰ïˆõ‚b‚c = B.‰ïˆõ‚b‚c \n");
				sbQuery.Append(" AND R.•”–å‚b‚c = B.•”–å‚b‚c \n");
				sbQuery.Append(" AND B.—X•Ö”Ô† = Y.—X•Ö”Ô†(+) \n");
				sbQuery.Append(" AND Y.“XŠ‚b‚c = T.“XŠ‚b‚c(+) \n");
				sbQuery.Append(" AND R.íœ‚e‚f = '0' \n");
				sbQuery.Append(" AND '0' = K.íœ‚e‚f \n");
				sbQuery.Append(" AND '0' = B.íœ‚e‚f \n");
				sbQuery.Append(" AND '0' = T.íœ‚e‚f(+) \n");
				sbQuery.Append(" AND R.‰ïˆõ‚b‚c = CM06.‰ïˆõ‚b‚c(+) \n");
				sbQuery.Append(" AND R.•”–å‚b‚c = CM06.•”–å‚b‚c(+) \n");
				sbQuery.Append(" AND K.ŠÇ—Ò‹æ•ª IN ('3','4') \n"); // 3:‰¤qˆê”Ê 4:‰¤q‰c‹ÆŠ

				
				if(sData[0].Length > 0 && sData[1].Length > 0)
					sbQuery.Append(" AND R.‰ïˆõ‚b‚c  BETWEEN '"+ sData[0] + "' AND '"+ sData[1] +"' \n");
				else
				{
					if(sData[0].Length > 0 && sData[1].Length == 0)
						sbQuery.Append(" AND R.‰ïˆõ‚b‚c = '"+ sData[0] + "' \n");
				}

				if(sData[2].Length > 0 && sData[3].Length > 0)
					sbQuery.Append(" AND K.g—pŠJn“ú  BETWEEN '"+ sData[2] + "' AND '"+ sData[3] +"' \n");
				else
				{
					if(sData[2].Length > 0 && sData[3].Length == 0)
						sbQuery.Append(" AND K.g—pŠJn“ú = '"+ sData[2] + "' \n");
				}

				if(sData[4].Length > 0 && sData[5].Length > 0)
					sbQuery.Append(" AND K.g—pI—¹“ú  BETWEEN '"+ sData[4] + "' AND '"+ sData[5] +"' \n");
				else
				{
					if(sData[4].Length > 0 && sData[5].Length == 0)
						sbQuery.Append(" AND K.g—pI—¹“ú = '"+ sData[4] + "' \n");
				}

				if(sData[6].Length > 0 && sData[7].Length > 0)
					sbQuery.Append(" AND SUBSTR(R.“o˜^“ú,1,8)  BETWEEN '"+ sData[6] + "' AND '"+ sData[7] +"' \n");
				else
				{
					if(sData[6].Length > 0 && sData[7].Length == 0)
						sbQuery.Append(" AND SUBSTR(R.“o˜^“ú,1,8) = '"+ sData[6] + "' \n");
				}
				sbQuery.Append(" ORDER BY R.‰ïˆõ‚b‚c,R.—˜—pÒ‚b‚c ");


				OracleDataReader reader;

				sbQuery2.Append(GET_KAIINCSV_SELECT);
				sbQuery2.Append(sbQuery);
				reader = CmdSelect(sUser, conn2, sbQuery2);

				StringBuilder sbData = new StringBuilder(1024);
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
					sbData.Append(sDbl + sSng + reader.GetString(0).Trim() + sDbl);				// ‰ïˆõ‚b‚c
					sbData.Append(sKanma + sDbl + reader.GetString(1).Trim() + sDbl);			// ‰ïˆõ–¼
					sbData.Append(sKanma + sDbl + reader.GetString(2).Trim() + sDbl);			// g—pŠJn“ú
					sbData.Append(sKanma + sDbl + reader.GetString(3).Trim() + sDbl);			// g—pI—¹“ú
					sbData.Append(sKanma + sDbl + reader.GetString(23).TrimEnd() + sDbl);		// ŠÇ—Ò‹æ•ª
					sbData.Append(sKanma + sDbl + reader.GetString(24).TrimEnd() + sDbl);		// ‰^’À”ñ•\¦i‹L–˜AŒg‚e‚fj
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(25).ToString().TrimEnd() + sDbl); // “o˜^“ú
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(26).ToString().TrimEnd() + sDbl); // XV“ú
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(4).Trim() + sDbl);	// •”–å‚b‚c
					sbData.Append(sKanma + sDbl + reader.GetString(5).Trim() + sDbl);			// •”–å–¼
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(6).Trim() + sDbl);	// ŠÇ—“XŠ‚b‚c
					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);			// ŠÇ—“XŠ–¼
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).Trim() + sDbl);	// İ’uæZŠ‚P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).Trim() + sDbl);	// İ’uæZŠ‚Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(27).TrimEnd() + sDbl);		// Ver.i‘gD‚b‚cj
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(28).TrimEnd() + sDbl);		// —X•Ö”Ô†
					sbData.Append(sKanma + sDbl + reader.GetDecimal(29).ToString().TrimEnd() + sDbl); // g—p—¿
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(30).TrimEnd() + sDbl); // ‰ïˆõ\ŠÇ—”Ô†
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(31).ToString().TrimEnd() + sDbl); // “o˜^“ú
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(32).ToString().TrimEnd() + sDbl); // XV“ú
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).Trim() + sDbl);	// —˜—pÒ‚b‚c
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).Trim() + sDbl);	// ƒpƒXƒ[ƒh
					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl       );	// —˜—pÒ–¼
					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);			// —˜—pÒ“o˜^“ú
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(33).TrimEnd() + sDbl); // ‰×‘—l‚b‚c
					sbData.Append(sKanma + sDbl + reader.GetString(34).TrimEnd() + sDbl);		 // ƒ‰ƒxƒ‹ˆóü‹Ö~
					sbData.Append(sKanma + sDbl + reader.GetDecimal(35).ToString().TrimEnd() + sDbl); // ”FØƒGƒ‰[‰ñ”
					sbData.Append(sKanma + sDbl + reader.GetString(36).TrimEnd() + sDbl); // ƒpƒXƒ[ƒhXV“úi“o˜^‚o‚fj
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(37).ToString().TrimEnd() + sDbl); // “o˜^“ú
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(38).ToString().TrimEnd() + sDbl); // XV“ú
					sbData.Append(sKanma + sDbl + reader.GetDecimal(14) + sDbl);			// ƒT[ƒ}ƒ‹‘ä”
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).Trim() + sDbl);	// ƒVƒŠƒAƒ‹”Ô†‚P
					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);			// ó‘Ô‚P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).Trim() + sDbl);	// ƒVƒŠƒAƒ‹”Ô†‚Q
					sbData.Append(sKanma + sDbl + reader.GetString(18).Trim() + sDbl);			// ó‘Ô‚Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(19).Trim() + sDbl);	// ƒVƒŠƒAƒ‹”Ô†‚R
					sbData.Append(sKanma + sDbl + reader.GetString(20).Trim() + sDbl);			// ó‘Ô‚R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(21).Trim() + sDbl);	// ƒVƒŠƒAƒ‹”Ô†‚S
					sbData.Append(sKanma + sDbl + reader.GetString(22).Trim() + sDbl);			// ó‘Ô‚S

					sList.Add(sbData);
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ‰ïˆõƒ}ƒXƒ^æ“¾
		 * ˆø”F‰ïˆõ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA‰ïˆõ‚b‚cA‰ïˆõ–¼Ag—pŠJn“úAŠÇ—Ò‹æ•ªAg—pI—¹“ú
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(7634):
		*/
		[WebMethod]
		public string[] Sel_MemberTn(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "‰ïˆõƒ}ƒXƒ^ŒŸõŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[8];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT CM01.‰ïˆõ‚b‚c "
					+       ",CM01.‰ïˆõ–¼ "
					+       ",CM01.g—pŠJn“ú "
					+       ",CM01.ŠÇ—Ò‹æ•ª "
					+       ",CM01.g—pI—¹“ú "
					+       ",CM01.XV“ú \n"
					+       ",CM01.‹L–˜AŒg‚e‚f \n"
					+  " FROM ‚b‚l‚O‚P‰ïˆõ CM01\n"
					+  "     ,‚b‚l‚O‚Q•”–å CM02\n"
					+  "     ,‚b‚l‚P‚S—X•Ö”Ô†‚i CM14\n" // ‰¤q‰^‘—‘Î‰
					+ " WHERE CM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
					+    "AND CM01.íœ‚e‚f = '0' \n"
					+    "AND CM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
					+    "AND CM02.íœ‚e‚f = '0' \n"
					+    "AND CM14.—X•Ö”Ô† = CM02.—X•Ö”Ô† \n"
					+    "AND CM14.“XŠ‚b‚c = '" + sKey[1] + "' \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				while (reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim();
					sRet[4] = reader.GetString(3).Trim();
					sRet[5] = reader.GetString(4).Trim();
					sRet[6] = reader.GetDecimal(5).ToString().Trim();
					sRet[7] = reader.GetString(6);
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if(iCnt == 1) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
					sRet[0] = "³íI—¹";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ‰ïˆõƒ}ƒXƒ^æ“¾
		 * ˆø”F‰ïˆõ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA‰ïˆõ‚b‚cA‰ïˆõ–¼Ag—pŠJn“úAŠÇ—Ò‹æ•ªAg—pI—¹“ú
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(127):
		*/
		[WebMethod]
		public string[] Sel_Member(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "‰ïˆõƒ}ƒXƒ^ŒŸõŠJn");

			OracleConnection conn2 = null;
			// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
			//			string[] sRet = new string[8];
			string[] sRet = new string[9];
			// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT ‰ïˆõ‚b‚c "
					+       ",‰ïˆõ–¼ "
					+       ",g—pŠJn“ú "
					+       ",ŠÇ—Ò‹æ•ª "
					+       ",g—pI—¹“ú "
					+       ",XV“ú \n"
					+       ",‹L–˜AŒg‚e‚f \n"
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					+       ",•Û—¯ˆóü‚e‚f \n"
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					+  " FROM ‚b‚l‚O‚P‰ïˆõ \n"
					// MOD 2011.06.01 “Œ“sj‚–Ø ‚r‚p‚k‚Ì’²® START
					//					+ " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
					//					+ " OR ‰ïˆõ‚b‚c = 'J" + sKey[0] + "' \n" // ‰¤q‰^‘—‘Î‰
					//					+    "AND íœ‚e‚f = '0' \n"
					+ " WHERE ( ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
					+ "  OR ‰ïˆõ‚b‚c = 'J" + sKey[0] + "' ) \n" // ‰¤q‰^‘—‘Î‰
					+ " AND íœ‚e‚f = '0' \n"
					+ " ORDER BY ‰ïˆõ‚b‚c \n"
					;
				// MOD 2011.06.01 “Œ“sj‚–Ø ‚r‚p‚k‚Ì’²® END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				while (reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim();
					sRet[4] = reader.GetString(3).Trim();
					sRet[5] = reader.GetString(4).Trim();
					sRet[6] = reader.GetDecimal(5).ToString().Trim();
					sRet[7] = reader.GetString(6);
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					sRet[8] = reader.GetString(7);
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if(iCnt == 1) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
					sRet[0] = "³íI—¹";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
		
		/*********************************************************************
		 * ‰ïˆõƒ}ƒXƒ^ˆê——æ“¾‚Q
		 * ˆø”F‰ïˆõ‚b‚cA‰ïˆõ–¼
		 * –ß’lFƒXƒe[ƒ^ƒXA‰ïˆõ‚b‚cA‰ïˆõ–¼
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(7741):
		*/
		[WebMethod]
		public string[] Get_MemberTn(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "‰ïˆõƒ}ƒXƒ^ˆê——æ“¾‚QŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT ‰ïˆõ.‰ïˆõî•ñ from ( "
					+ "SELECT '|' "
					+     "|| TRIM(CM01.‰ïˆõ‚b‚c) || '|' "
					+     "|| TRIM(CM01.‰ïˆõ–¼) || '|' "
					+     "|| TRIM(g—pI—¹“ú) || '|' "
					+     "|| TO_CHAR(SYSDATE,'YYYYMMDD') || '|' "
					+     "‰ïˆõî•ñ \n"
					+  " FROM ‚b‚l‚O‚P‰ïˆõ CM01\n"
					+  "     ,‚b‚l‚O‚Q•”–å CM02 \n"
					+  "     ,‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n"; // ‰¤q‰^‘—‘Î‰
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.‰ïˆõ‚b‚c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.‰ïˆõ–¼ LIKE '%" + sKey[1] + "%' \n";
				}

				cmdQuery += " AND CM01.ŠÇ—Ò‹æ•ª IN ('1','3','4') \n"; // 1:ŠÇ—Ò 3:‰¤qˆê”Ê 4:‰¤q‰c‹ÆŠ
				cmdQuery += " AND CM01.íœ‚e‚f = '0' \n";

				cmdQuery += " AND CM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n";
				cmdQuery += " AND CM02.íœ‚e‚f = '0' \n";
				cmdQuery += " AND CM14.—X•Ö”Ô† = CM02.—X•Ö”Ô† \n";
				if (sKey[2].Trim().Length != 0)
				{
					cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sKey[2] + "' \n";
				}
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.g—pI—¹“ú >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += " ORDER BY CM01.‰ïˆõ‚b‚c \n";
				cmdQuery += " ) ‰ïˆõ GROUP BY ‰ïˆõ.‰ïˆõî•ñ \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0)
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ‰ïˆõƒ}ƒXƒ^ˆê——æ“¾‚R
		 * ˆø”F‰ïˆõ‚b‚cA‰ïˆõ–¼
		 * –ß’lFƒXƒe[ƒ^ƒXA‰ïˆõ‚b‚cA‰ïˆõ–¼
		 *
		 * QÆŒ³F‰ïˆõŒŸõ‚Q.cs
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(7881):
		*/
		[WebMethod]
		public string[] Get_MemberTn3(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "‰ïˆõƒ}ƒXƒ^ˆê——æ“¾‚RŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' "
					+     "|| TRIM(CM01.‰ïˆõ‚b‚c) || '|' "
					+     "|| TRIM(CM01.‰ïˆõ–¼) || '|' "
					+     "|| TRIM(g—pI—¹“ú) || '|' "
					+     "|| TO_CHAR(SYSDATE,'YYYYMMDD') || '|' "
					+     "‰ïˆõî•ñ \n"
					+     ", CM01.‰ïˆõ‚b‚c kcd \n"
					+  " FROM ‚b‚l‚O‚P‰ïˆõ CM01\n";
				cmdQuery += "     ,‚b‚l‚O‚Q•”–å CM02 \n";
				cmdQuery += "     ,‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n"; // ‰¤q‰^‘—‘Î‰
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.‰ïˆõ‚b‚c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.‰ïˆõ–¼ LIKE '%" + sKey[1] + "%' \n";
				}
				cmdQuery += " AND CM01.ŠÇ—Ò‹æ•ª IN ('1','3','4') \n"; // 1:ŠÇ—Ò 3:‰¤qˆê”Ê 4:‰¤q‰c‹ÆŠ
				cmdQuery += " AND CM01.íœ‚e‚f = '0' \n";

				cmdQuery += " AND CM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
					+ " AND CM02.íœ‚e‚f = '0' \n"
					+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
					;
				if (sKey[2].Trim().Length != 0)
					cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sKey[2] + "' \n";
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.g—pI—¹“ú >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += "UNION \n";
				cmdQuery += "SELECT '|' "
					+ "|| TRIM(CM01.‰ïˆõ‚b‚c) || '|' "
					+ "|| TRIM(CM01.‰ïˆõ–¼) || '|' ‰ïˆõî•ñ \n"
					+ ", CM01.‰ïˆõ‚b‚c \n"
					+ " FROM ‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+ "     ,‚b‚l‚O‚T‰ïˆõˆµ“X CM05 \n";
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.‰ïˆõ‚b‚c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.‰ïˆõ–¼ LIKE '%" + sKey[1] + "%' \n";
				}
				cmdQuery += " AND CM01.ŠÇ—Ò‹æ•ª IN ('1','3','4') \n"; // 3:‰¤qˆê”Ê 4:‰¤q‰c‹ÆŠ
				cmdQuery += " AND CM01.íœ‚e‚f = '0' \n"
					+ " AND CM01.‰ïˆõ‚b‚c = CM05.‰ïˆõ‚b‚c \n"
					+ " AND CM05.íœ‚e‚f = '0' \n";
				if (sKey[2].Trim().Length != 0)
					cmdQuery += " AND CM05.“XŠ‚b‚c = '" + sKey[2] + "' \n";
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.g—pI—¹“ú >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += " ORDER BY kcd \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0)
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ‚²ˆË—Šåˆê——æ“¾iglobal‘Î‰j
		 * ˆø”F‰ïˆõ‚b‚cA‰×‘—l–¼A‰×‘—l‚b‚cA“XŠ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAˆê——i–¼‘O‚PAZŠ‚PA‰×‘—l‚b‚cj...
		 *
		 * QÆŒ³F‚²ˆË—ŠåŒŸõ‚Q.cs
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(8633):
		*/
		[WebMethod]
		public string[] Get_Goirainusi2(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "‚²ˆË—Šåˆê——æ“¾‚QŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' "
					+     "|| TRIM(SM01.‰ïˆõ‚b‚c) || '|' "
					+     "|| TRIM(CM01.‰ïˆõ–¼) || '|' "
					+     "|| TRIM(CM02.•”–å–¼) || '|' "
					+     "|| TRIM(SM01.‰×‘—l‚b‚c) || '|' "
					+     "|| TRIM(SM01.–¼‘O‚P) || '|' "
					+     "|| TRIM(SM01.ZŠ‚P) || '|' "
					+     "|| TRIM(SM01.•”–å‚b‚c) || '|' \n"
					+    ",CM01.‰ïˆõ‚b‚c kcd \n"
					+  " FROM ‚r‚l‚O‚P‰×‘—l SM01"
					+       ",‚b‚l‚O‚Q•”–å CM02"
					+       ",‚b‚l‚P‚S—X•Ö”Ô†‚i CM14" // ‰¤q‰^‘—‘Î‰
					+       ",‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+ " WHERE SM01.‰ïˆõ‚b‚c =  CM01.‰ïˆõ‚b‚c \n";
				if (sKey[0].Length == 10)
				{
					cmdQuery += " AND SM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " AND SM01.‰ïˆõ‚b‚c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Length == 12)
				{
					cmdQuery += " AND SM01.‰×‘—l‚b‚c = '" + sKey[1] + "' \n";
				}
				else
				{
					if (sKey[1].Length != 0)
					{
						cmdQuery += " AND SM01.‰×‘—l‚b‚c LIKE '" + sKey[1] + "%' \n";
					}
				}
				if (sKey[2].Length != 0)
				{
					cmdQuery += " AND SM01.–¼‘O‚P LIKE '%" + sKey[2] + "%' \n";
				}
				cmdQuery += " AND SM01.‰ïˆõ‚b‚c =  CM02.‰ïˆõ‚b‚c \n"
					+  " AND SM01.•”–å‚b‚c =  CM02.•”–å‚b‚c \n"
					+  " AND CM02.—X•Ö”Ô† =  CM14.—X•Ö”Ô† \n"
					;
				if (sKey[3].Length != 0)
				{
					cmdQuery += " AND CM14.“XŠ‚b‚c =  '" + sKey[3] + "' \n";
				}
				cmdQuery += " AND SM01.íœ‚e‚f = '0' \n"
					+  " AND CM02.íœ‚e‚f = '0' \n"
					+  " AND CM01.íœ‚e‚f = '0' \n";

				cmdQuery += "UNION \n";
				cmdQuery += "SELECT '|' "
					+     "|| TRIM(SM01.‰ïˆõ‚b‚c) || '|' "
					+     "|| TRIM(CM01.‰ïˆõ–¼) || '|' "
					+     "|| TRIM(CM02.•”–å–¼) || '|' "
					+     "|| TRIM(SM01.‰×‘—l‚b‚c) || '|' "
					+     "|| TRIM(SM01.–¼‘O‚P) || '|' "
					+     "|| TRIM(SM01.ZŠ‚P) || '|' "
					+     "|| TRIM(SM01.•”–å‚b‚c) || '|' \n"
					+    ",CM01.‰ïˆõ‚b‚c \n"
					+  " FROM ‚r‚l‚O‚P‰×‘—l SM01"
					+       ",‚b‚l‚O‚Q•”–å CM02"
					+       ",‚b‚l‚O‚T‰ïˆõˆµ“X CM05"
					+       ",‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+ " WHERE SM01.‰ïˆõ‚b‚c =  CM01.‰ïˆõ‚b‚c \n"
					+ "";
				if (sKey[0].Length == 10)
				{
					cmdQuery += " AND SM01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " AND SM01.‰ïˆõ‚b‚c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Length == 12)
				{
					cmdQuery += " AND SM01.‰×‘—l‚b‚c = '" + sKey[1] + "' \n";
				}
				else
				{
					if (sKey[1].Length != 0)
					{
						cmdQuery += " AND SM01.‰×‘—l‚b‚c LIKE '" + sKey[1] + "%' \n";
					}
				}
				if (sKey[2].Length != 0)
				{
					cmdQuery += " AND SM01.–¼‘O‚P LIKE '%" + sKey[2] + "%' \n";
				}
				cmdQuery += " AND SM01.‰ïˆõ‚b‚c =  CM02.‰ïˆõ‚b‚c \n"
					+  " AND SM01.•”–å‚b‚c =  CM02.•”–å‚b‚c \n"
					+  " AND SM01.‰ïˆõ‚b‚c =  CM05.‰ïˆõ‚b‚c \n"
					;
				if (sKey[3].Length != 0)
				{
					cmdQuery += " AND CM05.“XŠ‚b‚c =  '" + sKey[3] + "' \n";
				}
				cmdQuery += " AND SM01.íœ‚e‚f = '0' \n"
					+  " AND CM02.íœ‚e‚f = '0' \n"
					+  " AND CM05.íœ‚e‚f = '0' \n"
					+  " AND CM01.íœ‚e‚f = '0' \n";
				cmdQuery += "ORDER BY kcd \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ˆË—Šåƒf[ƒ^æ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA‰×‘—l‚b‚cA“XŠ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAƒJƒi—ªÌA“d˜b”Ô†A—X•Ö”Ô†AZŠA–¼‘OAd—Ê
		 *		 ƒ[ƒ‹ƒAƒhƒŒƒXA“¾ˆÓæ‚b‚cA“¾ˆÓæ•”‰Û‚b‚cAXV“ú
		 *********************************************************************/
		private static string GET_SIRAINUSI_SELECT1
			= "SELECT SM01.–¼‘O‚P \n"
			+ " FROM ‚r‚l‚O‚P‰×‘—l SM01 \n"
			+ ", ‚b‚l‚O‚Q•”–å CM02 \n"
			+ ", ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
			+ "";

		private static string GET_SIRAINUSI_SELECT2
			= "SELECT CM02.•”–å–¼ \n"
			+ " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
			+ ", ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
			+ "";

		private static string GET_SIRAINUSI_SELECT3
			= "SELECT CM01.‰ïˆõ–¼ \n"
			+ " FROM ‚b‚l‚O‚P‰ïˆõ CM01 \n"
			+ ", ‚b‚l‚O‚Q•”–å CM02 \n"
			+ ", ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
			+ "";
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(10314):
		*/
		/*
				[WebMethod]
				public String[] Get_Sirainusi(string[] sUser, string sKCode, string sBCode, string sICode, string sTCode)
				{
					logWriter(sUser, INF, "ˆË—Šåî•ñæ“¾ŠJn");

					OracleConnection conn2 = null;
					string[] sRet = new string[4]{"","","",""};

					// ‚c‚aÚ‘±
					conn2 = connect2(sUser);
					if(conn2 == null)
					{
						sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
						return sRet;
					}
					try
					{
						string cmdQuery = "";
						OracleDataReader reader;

						if(sKCode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT3
								+ " WHERE CM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
								+ " AND CM01.íœ‚e‚f = '0' \n"
								+ " AND CM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
								+ " AND CM02.íœ‚e‚f = '0' \n"
								+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
								+ "";

							//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[1]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
							if(sBCode.Length > 0)
							{
								cmdQuery = GET_SIRAINUSI_SELECT2
									+ " WHERE CM02.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
									+ " AND CM02.•”–å‚b‚c = '" + sBCode + "' \n"
									+ " AND CM02.íœ‚e‚f = '0' \n"
									+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
									+ "";

								//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
								if(sTCode.Length > 0)
								{
									cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
								}

								reader = CmdSelect(sUser, conn2, cmdQuery);

								if(reader.Read()) sRet[2]  = reader.GetString(0).Trim();
								disposeReader(reader);
								reader = null;

								if(sICode.Length > 0)
								{
									cmdQuery = GET_SIRAINUSI_SELECT1
										+ " WHERE SM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
										+ " AND SM01.•”–å‚b‚c = '" + sBCode + "' \n"
										+ " AND SM01.‰×‘—l‚b‚c = '" + sICode + "' \n"
										+ " AND SM01.íœ‚e‚f = '0' \n"
										+ " AND SM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
										+ " AND SM01.•”–å‚b‚c = CM02.•”–å‚b‚c \n"
										+ " AND CM02.íœ‚e‚f = '0' \n"
										+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
										+ "";

									//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
									if(sTCode.Length > 0)
									{
										cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
									}

									reader = CmdSelect(sUser, conn2, cmdQuery);

									if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
									disposeReader(reader);
									reader = null;
								}
							}
							else
							{
								//•”–å‚b‚c‚ª–¢“ü—Í‚Ìê‡
								if(sICode.Length > 0)
								{
									cmdQuery = GET_SIRAINUSI_SELECT1
										+ " WHERE SM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
										+ " AND SM01.‰×‘—l‚b‚c = '" + sICode + "' \n"
										+ " AND SM01.íœ‚e‚f = '0' \n"
										+ " AND SM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
										+ " AND SM01.•”–å‚b‚c = CM02.•”–å‚b‚c \n"
										+ " AND CM02.íœ‚e‚f = '0' \n"
										+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
										+ "";

									//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
									if(sTCode.Length > 0)
									{
										cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
									}

									reader = CmdSelect(sUser, conn2, cmdQuery);

									if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
									disposeReader(reader);
									reader = null;
								}
							}
						}

						sRet[0] = "³íI—¹";
						logWriter(sUser, INF, sRet[0]);
					}
					catch (OracleException ex)
					{
						sRet[0] = chgDBErrMsg(sUser, ex);
					}
					catch (Exception ex)
					{
						sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
						logWriter(sUser, ERR, sRet[0]);
					}
					finally
					{
						disconnect2(sUser, conn2);
						conn2 = null;
					}
			
					return sRet;
				}
		*/
		/*********************************************************************
		 * ˆË—Šåî•ñæ“¾‚Q
		 * ˆø”Fƒ†[ƒU[A‰ïˆõ‚b‚cA•”–å‚b‚cA‰×‘—l‚b‚cA“XŠ‚b‚c
		 * –ß’lFˆË—Šåî•ñ
		 *
		 * QÆŒ³Fo‰×Æ‰ï.cs
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(10479):
		*/
		[WebMethod]
		public String[] Get_Sirainusi2(string[] sUser, string sKCode, string sBCode, string sICode, string sTCode)
		{
			logWriter(sUser, INF, "ˆË—Šåî•ñæ“¾‚QŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[4]{"","","",""};

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			try
			{
				string cmdQuery = "";
				OracleDataReader reader;

				if(sKCode.Length > 0)
				{
					cmdQuery = GET_SIRAINUSI_SELECT3
						+ " WHERE CM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
						+ " AND CM01.íœ‚e‚f = '0' \n"
						+ " AND CM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
						+ " AND CM02.íœ‚e‚f = '0' \n"
						+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
						+ "";

					//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
					if(sTCode.Length > 0)
					{
						cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
					}
					//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
					if (sTCode.Length > 0) 
					{
						cmdQuery += "UNION \n";
						cmdQuery += "SELECT CM01.‰ïˆõ–¼ \n"
							+ " FROM ‚b‚l‚O‚P‰ïˆõ CM01 \n"
							+ "     ,‚b‚l‚O‚T‰ïˆõˆµ“X CM05 \n"
							+ " WHERE CM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
							+ " AND CM01.íœ‚e‚f = '0' \n"
							+ " AND CM01.‰ïˆõ‚b‚c = CM05.‰ïˆõ‚b‚c \n"
							+ " AND CM05.íœ‚e‚f = '0' \n"
							+ " AND CM05.“XŠ‚b‚c = '" + sTCode + "' \n";
					}

					reader = CmdSelect(sUser, conn2, cmdQuery);

					if(reader.Read()) sRet[1]  = reader.GetString(0).Trim();
					disposeReader(reader);
					reader = null;

					if(sBCode.Length > 0)
					{
						cmdQuery = GET_SIRAINUSI_SELECT2
							+ " WHERE CM02.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
							+ " AND CM02.•”–å‚b‚c = '" + sBCode + "' \n"
							+ " AND CM02.íœ‚e‚f = '0' \n"
							+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
							+ "";

						//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
						if(sTCode.Length > 0)
						{
							cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
						}

						//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
						if (sTCode.Length > 0) 
						{
							cmdQuery += "UNION \n";
							cmdQuery += "SELECT CM02.•”–å–¼ \n"
								+ " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
								+ "     ,‚b‚l‚O‚T‰ïˆõˆµ“X CM05 \n"
								+ " WHERE CM02.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
								+ " AND CM02.•”–å‚b‚c = '" + sBCode + "' \n"
								+ " AND CM02.íœ‚e‚f = '0' \n"
								+ " AND CM02.‰ïˆõ‚b‚c = CM05.‰ïˆõ‚b‚c \n"
								+ " AND CM05.íœ‚e‚f = '0' \n"
								+ " AND CM05.“XŠ‚b‚c = '" + sTCode + "' \n";
						}

						reader = CmdSelect(sUser, conn2, cmdQuery);

						if(reader.Read()) sRet[2]  = reader.GetString(0).Trim();
						disposeReader(reader);
						reader = null;

						if(sICode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT1
								+ " WHERE SM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
								+ " AND SM01.•”–å‚b‚c = '" + sBCode + "' \n"
								+ " AND SM01.‰×‘—l‚b‚c = '" + sICode + "' \n"
								+ " AND SM01.íœ‚e‚f = '0' \n"
								+ " AND SM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
								+ " AND SM01.•”–å‚b‚c = CM02.•”–å‚b‚c \n"
								+ " AND CM02.íœ‚e‚f = '0' \n"
								+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
								+ "";

							//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
							}

							//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
							if (sTCode.Length > 0) 
							{
								cmdQuery += "UNION \n";
								cmdQuery += "SELECT SM01.–¼‘O‚P \n"
									+ " FROM ‚r‚l‚O‚P‰×‘—l SM01 \n"
									+ "     ,‚b‚l‚O‚T‰ïˆõˆµ“X CM05 \n"
									+ " WHERE SM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
									+ " AND SM01.•”–å‚b‚c = '" + sBCode + "' \n"
									+ " AND SM01.‰×‘—l‚b‚c = '" + sICode + "' \n"
									+ " AND SM01.íœ‚e‚f = '0' \n"
									+ " AND SM01.‰ïˆõ‚b‚c = CM05.‰ïˆõ‚b‚c \n"
									+ " AND CM05.íœ‚e‚f = '0' \n"
									+ " AND CM05.“XŠ‚b‚c = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
						}
					}
					else
					{
						//•”–å‚b‚c‚ª–¢“ü—Í‚Ìê‡
						if(sICode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT1
								+ " WHERE SM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
								+ " AND SM01.‰×‘—l‚b‚c = '" + sICode + "' \n"
								+ " AND SM01.íœ‚e‚f = '0' \n"
								+ " AND SM01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
								+ " AND SM01.•”–å‚b‚c = CM02.•”–å‚b‚c \n"
								+ " AND CM02.íœ‚e‚f = '0' \n"
								+ " AND CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n"
								+ "";

							//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.“XŠ‚b‚c = '" + sTCode + "' \n";
							}

							//“XŠ‚b‚c‚ªİ’è‚³‚ê‚Ä‚¢‚é
							if (sTCode.Length > 0) 
							{
								cmdQuery += "UNION \n";
								cmdQuery += "SELECT SM01.–¼‘O‚P \n"
									+ " FROM ‚r‚l‚O‚P‰×‘—l SM01 \n"
									+ "     ,‚b‚l‚O‚T‰ïˆõˆµ“X CM05 \n"
									+ " WHERE SM01.‰ïˆõ‚b‚c = '" + sKCode + "' \n"
									+ " AND SM01.‰×‘—l‚b‚c = '" + sICode + "' \n"
									+ " AND SM01.íœ‚e‚f = '0' \n"
									+ " AND SM01.‰ïˆõ‚b‚c = CM05.‰ïˆõ‚b‚c \n"
									+ " AND CM05.íœ‚e‚f = '0' \n"
									+ " AND CM05.“XŠ‚b‚c = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
						}
					}
				}

				sRet[0] = "³íI—¹";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * o‰×ˆóüƒf[ƒ^æ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA“o˜^“úAƒWƒƒ[ƒiƒ‹‚m‚n
		 * –ß’lFƒXƒe[ƒ^ƒXA‰×ól‚b‚cA“d˜b”Ô†AZŠ...
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2print\Service1.asmx.cs(101):
		*/
		[WebMethod]
		public String[] Get_InvoicePrintData(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "o‰×ˆóüƒf[ƒ^æ“¾ŠJn");

			OracleConnection conn2 = null;
			// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü START
			//			string[] sRet = new string[40];
			// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
			//			string[] sRet = new string[41];
			// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
			//			string[] sRet = new string[42];
			// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš START
			//			string[] sRet = new string[45];
			string[] sRet = new string[46];
			// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš END
			// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
			// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
			// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü END
			// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ START
			string s—˜—pÒ•”–å“XŠ‚b‚c = (sKey.Length >  4) ? sKey[ 4] : "";
			// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ END
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			decimal dË” = 0;
			string s—X•Ö”Ô† = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT ");
				sbQuery.Append(" ST01.‰×ól‚b‚c ");
				sbQuery.Append(",ST01.“d˜b”Ô†‚P ");
				sbQuery.Append(",ST01.“d˜b”Ô†‚Q ");
				sbQuery.Append(",ST01.“d˜b”Ô†‚R ");
				sbQuery.Append(",ST01.ZŠ‚P ");
				sbQuery.Append(",ST01.ZŠ‚Q ");
				sbQuery.Append(",ST01.ZŠ‚R ");
				sbQuery.Append(",ST01.–¼‘O‚P ");
				sbQuery.Append(",ST01.–¼‘O‚Q ");
				sbQuery.Append(",ST01.o‰×“ú ");
				sbQuery.Append(",ST01.‘—‚èó”Ô† ");
				sbQuery.Append(",ST01.—X•Ö”Ô† ");
				sbQuery.Append(",ST01.’…“X‚b‚c ");
				sbQuery.Append(",NVL(CM14.“XŠ‚b‚c, ST01.”­“X‚b‚c)");
				sbQuery.Append(",SM01.“d˜b”Ô†‚P ");
				sbQuery.Append(",SM01.“d˜b”Ô†‚Q ");
				sbQuery.Append(",SM01.“d˜b”Ô†‚R ");
				sbQuery.Append(",SM01.ZŠ‚P ");
				sbQuery.Append(",SM01.ZŠ‚Q ");
				sbQuery.Append(",SM01.ZŠ‚R ");
				sbQuery.Append(",SM01.–¼‘O‚P ");
				sbQuery.Append(",SM01.–¼‘O‚Q ");
				sbQuery.Append(",ST01.ŒÂ” ");
				sbQuery.Append(",ST01.d—Ê ");
				sbQuery.Append(",ST01.•ÛŒ¯‹àŠz ");
				sbQuery.Append(",ST01.w’è“ú ");
				sbQuery.Append(",ST01.—A‘—w¦‚P ");
				sbQuery.Append(",ST01.—A‘—w¦‚Q ");
				sbQuery.Append(",ST01.•i–¼‹L–‚P ");
				sbQuery.Append(",ST01.•i–¼‹L–‚Q ");
				sbQuery.Append(",ST01.•i–¼‹L–‚R ");
				sbQuery.Append(",ST01.Œ³’…‹æ•ª ");
				sbQuery.Append(",ST01.‘—‚èó”­sÏ‚e‚f ");
				sbQuery.Append(",ST01.Ë” \n");
				sbQuery.Append(",ST01.‰×‘—l•”–¼ ");
				sbQuery.Append(",ST01.‚¨‹q—lo‰×”Ô† ");
				sbQuery.Append(",ST01.—A‘—w¦‚b‚c‚P ");
				sbQuery.Append(",ST01.—A‘—w¦‚b‚c‚Q ");
				sbQuery.Append(",ST01.w’è“ú‹æ•ª ");
				sbQuery.Append(",ST01.—X•Ö”Ô† ");
				sbQuery.Append(",ST01.d•ª‚b‚c ");
				sbQuery.Append(",NVL(CM10.“XŠ–¼, ST01.”­“X–¼)");
				sbQuery.Append(",ST01.o‰×Ï‚e‚f ");
				// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü START
				sbQuery.Append(",SM01.—X•Ö”Ô† ");
				// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü END
				// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
				sbQuery.Append(",NVL(CM01.•Û—¯ˆóü‚e‚f,'0') \n");
				// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
				// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
				sbQuery.Append(",ST01.•i–¼‹L–‚S ,ST01.•i–¼‹L–‚T ,ST01.•i–¼‹L–‚U \n");
				// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
				// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš START
				sbQuery.Append(",ST01.’…“X–¼ ");
				// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš END
				sbQuery.Append(" FROM \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" ST01");
				sbQuery.Append("\n");
				sbQuery.Append(" LEFT JOIN ‚b‚l‚O‚Q•”–å CM02 \n");
				sbQuery.Append(" ON ST01.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n");
				sbQuery.Append("AND ST01.•”–å‚b‚c = CM02.•”–å‚b‚c \n");
				sbQuery.Append(" LEFT JOIN ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n"); // ‰¤q‰^‘—‘Î‰
				sbQuery.Append(" ON CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n");
				sbQuery.Append(" LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n");
				sbQuery.Append(" ON CM14.“XŠ‚b‚c = CM10.“XŠ‚b‚c \n");
				// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
				sbQuery.Append(" LEFT JOIN ‚b‚l‚O‚P‰ïˆõ CM01 \n");
				sbQuery.Append(" ON ST01.‰ïˆõ‚b‚c = CM01.‰ïˆõ‚b‚c \n");
				// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
				sbQuery.Append(", \"‚r‚l‚O‚P‰×‘—l\" SM01 \n");
				sbQuery.Append(" WHERE ST01.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND ST01.•”–å‚b‚c = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND ST01.“o˜^“ú = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND ST01.ƒWƒƒ[ƒiƒ‹‚m‚n = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND ST01.‰ïˆõ‚b‚c = SM01.‰ïˆõ‚b‚c \n");
				sbQuery.Append(" AND ST01.•”–å‚b‚c = SM01.•”–å‚b‚c \n");
				sbQuery.Append(" AND ST01.‰×‘—l‚b‚c = SM01.‰×‘—l‚b‚c \n");
				sbQuery.Append(" AND ST01.íœ‚e‚f = '0' \n");
				sbQuery.Append(" AND SM01.íœ‚e‚f = '0' \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				int iCnt = 0;
				if (reader.Read())
				{
					string s—A‘—¤•i‚b‚c‚P = reader.GetString(36).Trim();
					string s—A‘—¤•i‚b‚c‚Q = reader.GetString(37).Trim();
					sRet[1]  = reader.GetString(0).Trim();
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ START
					//					sRet[5]  = reader.GetString(4).Trim();
					//					sRet[6]  = reader.GetString(5).Trim();
					//					sRet[7]  = reader.GetString(6).Trim();
					//					sRet[8]  = reader.GetString(7).Trim();
					//					sRet[9]  = reader.GetString(8).Trim();
					sRet[5]  = reader.GetString(4).TrimEnd(); // ‰×ólZŠ‚P
					sRet[6]  = reader.GetString(5).TrimEnd(); // ‰×ólZŠ‚Q
					sRet[7]  = reader.GetString(6).TrimEnd(); // ‰×ólZŠ‚R
					sRet[8]  = reader.GetString(7).TrimEnd(); // ‰×ól–¼‘O‚P
					sRet[9]  = reader.GetString(8).TrimEnd(); // ‰×ól–¼‘O‚Q
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ END
					sRet[10] = reader.GetString(9).Trim();
					sRet[11] = reader.GetString(10).Trim();
					sRet[12] = reader.GetString(11).Trim();
					sRet[13] = reader.GetString(12).Trim().PadLeft(4, '0');
					sRet[14] = reader.GetString(13).Trim().PadLeft(4, '0');
					sRet[15] = reader.GetString(14).Trim();
					sRet[16] = reader.GetString(15).Trim();
					sRet[17] = reader.GetString(16).Trim();
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ START
					//					sRet[18] = reader.GetString(17).Trim();
					//					sRet[19] = reader.GetString(18).Trim();
					//					sRet[20] = reader.GetString(19).Trim();
					//					sRet[21] = reader.GetString(20).Trim();
					//					sRet[22] = reader.GetString(21).Trim();
					sRet[18] = reader.GetString(17).TrimEnd(); // ‰×‘—lZŠ‚P
					sRet[19] = reader.GetString(18).TrimEnd(); // ‰×‘—lZŠ‚Q
					sRet[20] = reader.GetString(19).TrimEnd(); // ‰×‘—lZŠ‚R
					sRet[21] = reader.GetString(20).TrimEnd(); // ‰×‘—l–¼‘O‚P
					sRet[22] = reader.GetString(21).TrimEnd(); // ‰×‘—l–¼‘O‚Q
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ END
					sRet[23] = reader.GetDecimal(22).ToString().Trim();
					// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ START
					//					dË”    = reader.GetDecimal(33);
					//					dË”    = dË” * 8;
					//					if(dË” == 0)
					//						sRet[24] = reader.GetDecimal(23).ToString().Trim();
					//					else
					//						sRet[24] = dË”.ToString().Trim();
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					if(reader.GetString(44) == "1")
					{
						dË” = reader.GetDecimal(33) * 8;
						if(dË” == 0)
						{
							sRet[24] = reader.GetDecimal(23).ToString().TrimEnd();
						}
						else
						{
							sRet[24] = dË”.ToString().TrimEnd();
						}
					}
					else
					{
						// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
						sRet[24] = "";
						// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					}
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ END
					sRet[25] = reader.GetDecimal(24).ToString().Trim();
					sRet[26] = reader.GetString(25).Trim();
					if (s—A‘—¤•i‚b‚c‚P.Equals("100"))
					{
						sRet[27] = reader.GetString(27).TrimEnd();
						sRet[28] = "";
					}
						// ‚Ps–Ú‚Æ‚Qs–Ú‚ª“¯‚¶ƒR[ƒh‚Ìê‡A‚Qs–Ú‚ğ•\¦‚µ‚È‚¢
					else if (s—A‘—¤•i‚b‚c‚P.Equals(s—A‘—¤•i‚b‚c‚Q))
					{
						sRet[27] = reader.GetString(26).TrimEnd();
						sRet[28] = "";
					}
					else
					{
						sRet[27] = reader.GetString(26).TrimEnd();
						sRet[28] = reader.GetString(27).TrimEnd();
					}
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ START
					//					sRet[29] = reader.GetString(28).Trim();
					//					sRet[30] = reader.GetString(29).Trim();
					//					sRet[31] = reader.GetString(30).Trim();
					sRet[29] = reader.GetString(28).TrimEnd(); // •i–¼‹L–‚P
					sRet[30] = reader.GetString(29).TrimEnd(); // •i–¼‹L–‚Q
					sRet[31] = reader.GetString(30).TrimEnd(); // •i–¼‹L–‚R
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ END
					// ƒp[ƒZƒ‹‚Ìê‡A"11"
					if (s—A‘—¤•i‚b‚c‚P.Equals("001") || s—A‘—¤•i‚b‚c‚P.Equals("002"))
						sRet[32] = reader.GetString(31).Trim() + "1";
					else
						sRet[32] = reader.GetString(31).Trim() + "0";
					sRet[33] = reader.GetString(32).Trim();
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ START
					//					sRet[34] = reader.GetString(34).Trim();
					sRet[34] = reader.GetString(34).TrimEnd(); // ’S“–Òi•”j
					// MOD 2011.01.18 “Œ“sj‚–Ø ZŠ–¼‘O‚Ì‘OSPACE‚ğ‚Â‚ß‚È‚¢ END
					sRet[35] = reader.GetString(35).Trim(); // ‚¨‹q—l”Ô†
					sRet[36] = reader.GetString(38).Trim();
					s—X•Ö”Ô† = reader.GetString(39).Trim();
					sRet[37] = reader.GetString(40).Trim();		//d•ª‚b‚c
					sRet[38] = reader.GetString(41).Trim();		//”­“X–¼
					sRet[39] = reader.GetString(42).Trim();		//o‰×Ï‚e‚f
					// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü START
					sRet[40] = reader.GetString(43).Trim();		//‚²ˆË—Šå—X•Ö”Ô†
					// MOD 2011.01.06 “Œ“sj‚–Ø —X•Ö”Ô†‚Ìˆóü END
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					sRet[41] = reader.GetString(44).TrimEnd();
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
					sRet[42] = reader.GetString(45).TrimEnd(); // •i–¼‹L–‚S
					sRet[43] = reader.GetString(46).TrimEnd(); // •i–¼‹L–‚T
					sRet[44] = reader.GetString(47).TrimEnd(); // •i–¼‹L–‚U
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
					// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš START
					sRet[45] = reader.GetString(48).TrimEnd(); // ’…“X–¼
					// MOD 2011.12.06 “Œ“sj‚–Ø ƒ‰ƒxƒ‹ƒwƒbƒ_•”‚É”­“X–¼E’…“X–¼‚ğˆóš END
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if (iCnt == 0)
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				else
				{
					sRet[0] = "³íI—¹";
					// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ START
					if(s—˜—pÒ•”–å“XŠ‚b‚c.Length == 0)
					{
						// MOD 2011.10.06 “Œ“sj‚–Ø o‰×ƒf[ƒ^‚ÌˆóüƒƒO‚Ì’Ç‰Á START
						logWriter(sUser, INF, "o‰×ˆóüƒf[ƒ^æ“¾@—˜—pÒ•”–å“XŠ‚b‚c–³"
							+"["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sRet[11]+"]"
							+"‘—‚èó”­sÏ["+sRet[33]+"]o‰×Ï["+sRet[39]+"]"
							);
						// MOD 2011.10.06 “Œ“sj‚–Ø o‰×ƒf[ƒ^‚ÌˆóüƒƒO‚Ì’Ç‰Á END
						return sRet;
					}
					// —˜—pÒ‚Ì•”–å‚ÌŠÇŠ“XŠ‚b‚c‚Æ“o˜^Ò‚Ì”­“X‚b‚c‚Æ‚ªˆÙ‚È‚éê‡
					string s”­“X‚b‚c = sRet[14].Trim().Substring(1, 3);
					if(!s”­“X‚b‚c.Equals(s—˜—pÒ•”–å“XŠ‚b‚c))
					{
						return sRet;
					}
					// ‘—‚èó”Ô†‚ª‚È‚¢ê‡‚É‚Íæ“¾‚·‚é
					if(sRet[11].Length == 0)
					{
						disconnect2(sUser, conn2);
						conn2 = null;

						string[] sRetInvoiceNo = Set_InvoiceNo2(sUser ,sKey, sRet, s—˜—pÒ•”–å“XŠ‚b‚c);
						if(sRetInvoiceNo[0].Length == 4)
						{
							//							sRet[11] = sRetInvoiceNo[1];
						}
						else
						{
							sRet[0] = sRetInvoiceNo[0];
						}
					}
					// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ END
					// MOD 2011.10.06 “Œ“sj‚–Ø o‰×ƒf[ƒ^‚ÌˆóüƒƒO‚Ì’Ç‰Á START
					logWriter(sUser, INF, "o‰×ˆóüƒf[ƒ^æ“¾"
						+"["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sRet[11]+"]"
						+"‘—‚èó”­sÏ["+sRet[33]+"]o‰×Ï["+sRet[39]+"]"
						);
					// MOD 2011.10.06 “Œ“sj‚–Ø o‰×ƒf[ƒ^‚ÌˆóüƒƒO‚Ì’Ç‰Á END
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * ‘—‚èó”­sÏ‚e‚f‚ÌXV
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA“o˜^“úAƒWƒƒ[ƒiƒ‹‚m‚nA‘—‚èó”Ô†AXVÒ
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2print\Service1.asmx.cs(778):
		*/
		[WebMethod]
		public String[] Set_InvoiceNo(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "”­sÏ‚e‚fXVŠJn");

			OracleConnection conn2 = null;
			// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ START
			//			string[] sRet = new string[1];
			string[] sRet = new string[2]{"",""};
			// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ END
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string s”­“X‚b‚c = "";
				string s”­“X–¼   = "";
				sbQuery.Append("SELECT NVL(CM14.“XŠ‚b‚c, ' ') \n");
				sbQuery.Append(", NVL(CM10.“XŠ–¼, ' ') \n");
				sbQuery.Append(" FROM ‚b‚l‚O‚Q•”–å CM02 \n");
				sbQuery.Append(" LEFT JOIN ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n"); // ‰¤q‰^‘—‘Î‰
				sbQuery.Append(" ON CM02.—X•Ö”Ô† = CM14.—X•Ö”Ô† \n");
				sbQuery.Append(" LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n");
				sbQuery.Append(" ON CM14.“XŠ‚b‚c = CM10.“XŠ‚b‚c \n");
				sbQuery.Append(" WHERE CM02.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND CM02.•”–å‚b‚c = '" + sKey[1] + "' \n");
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s”­“X‚b‚c = reader.GetString(0).Trim();
					s”­“X–¼   = reader.GetString(1).Trim();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ START
				// ‘—‚èó”Ô†ƒ`ƒFƒbƒN
				sbQuery = new StringBuilder(1024);
				string s‘—‚èó”Ô† = "";
				sbQuery.Append("SELECT ‘—‚èó”Ô† \n");
				sbQuery.Append(" FROM  \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n");
				sbQuery.Append(" WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND •”–å‚b‚c = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND “o˜^“ú   = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND \"ƒWƒƒ[ƒiƒ‹‚m‚n\" = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND íœ‚e‚f = '0' \n");
				sbQuery.Append(" FOR UPDATE \n");
				reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s‘—‚èó”Ô† = reader.GetString(0).TrimEnd();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				if(s‘—‚èó”Ô†.Length > 0)
				{
					// ˆÙ‚È‚é‘—‚èó”Ô†‚ğã‘‚«‚µ‚æ‚¤‚Æ‚µ‚½ê‡
					if(s‘—‚èó”Ô† != sKey[4])
					{
						tran.Commit();
						sRet[0] = "ƒGƒ‰[F‘¼‚Ì’[––‚Åˆóü’†‚à‚µ‚­‚ÍˆóüÏ‚Å‚·\n"
							+ "["+s‘—‚èó”Ô†.Substring(4)+"]";
						sRet[1] = s‘—‚èó”Ô†;
						logWriter(sUser, INF, "‘—‚èó”Ô†XVÏ["+sRet[1]+"]"
							+ " ["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sKey[4]+"]");
						return sRet;
					}
				}

				// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ END
				// o‰×ƒWƒƒ[ƒiƒ‹‚ÌXV
				string cmdQuery  = "UPDATE \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n";
				cmdQuery += " SET ‘—‚èó”Ô† = '"  + sKey[4] + "' ";                     // ‘—‚èó”Ô†
				// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ START
				cmdQuery +=     ",ˆ—‚O‚P = TO_CHAR(SYSDATE,'MMDDHH24MISS') \n"; // ‘—‚èóˆóüŒ“ú•ª•b
				// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ END
				cmdQuery +=     ",‘—‚èó”­sÏ‚e‚f = '1' ";
				cmdQuery +=     ",ó‘Ô = DECODE(ó‘Ô,'01','02','02','02',ó‘Ô) ";
				cmdQuery +=     ",Ú×ó‘Ô = DECODE(ó‘Ô,'01','  ','02','  ',Ú×ó‘Ô) ";
				cmdQuery +=     ",XV“ú =   TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";    // XV“ú
				cmdQuery +=     ",XV‚o‚f = 'o‰×“o˜^' ";                               // XV‚o‚f
				cmdQuery +=     ",XVÒ = '" + sKey[5] + "' \n";                        // XVÒ
				if(s”­“X‚b‚c.Length > 0)
				{
					cmdQuery += ",”­“X‚b‚c = '" + s”­“X‚b‚c + "' \n";
				}
				if(s”­“X–¼.Length > 0)
				{
					cmdQuery += ",”­“X–¼ = '"   + s”­“X–¼   + "' \n";
				}
				cmdQuery += " WHERE ‰ïˆõ‚b‚c       = '" + sKey[0] + "' \n";
				cmdQuery +=   " AND •”–å‚b‚c       = '" + sKey[1] + "' \n";
				cmdQuery +=   " AND “o˜^“ú         = '" + sKey[2] + "' \n";
				cmdQuery +=   " AND ƒWƒƒ[ƒiƒ‹‚m‚n = '" + sKey[3] + "' \n";
				cmdQuery +=   " AND íœ‚e‚f       = '0' \n";
				logWriter(sUser, INF, "”­sÏ‚e‚fXV["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sKey[4]+"]");

				CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "³íI—¹";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
		// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ START
		/*********************************************************************
		 * Ì”Ô‚ÌXV
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2print\Service1.asmx.cs(494):
		*/
		[WebMethod]
		public String[] Get_InvoiceNo(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "Ì”ÔXVŠJn");
			
			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			//ƒgƒ‰ƒ“ƒUƒNƒVƒ‡ƒ“‚Ìİ’è
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				decimal i“o˜^˜A”Ô     = 0;
				decimal iŠJnŒ´•[”Ô† = 0;
				decimal iI—¹Œ´•[”Ô† = 0;
				decimal iÅIŒ´•[”Ô† = 0;
				string  sŠ„•t“ú       = "";
				string  s—LŒøŠúŒÀ     = "";
				string  s“–“ú“ú•t     = "";

				string cmdQuery_am12 = "SELECT";
				cmdQuery_am12 += " AM12.“o˜^˜A”Ô ";
				cmdQuery_am12 += ",AM12.ŠJnŒ´•[”Ô† ";
				cmdQuery_am12 += ",AM12.I—¹Œ´•[”Ô† ";
				cmdQuery_am12 += ",AM12.ÅIŒ´•[”Ô† ";
				cmdQuery_am12 += ",AM12.Š„•t“ú ";
				cmdQuery_am12 += ",AM12.—LŒøŠúŒÀ ";
				cmdQuery_am12 += ",TO_CHAR(SYSDATE,'YYYYMMDD') \n";
				cmdQuery_am12 += " FROM ‚`‚l‚P‚Q‘—‚èóÌ”Ô AM12 \n";
				cmdQuery_am12 += " WHERE AM12.‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
				cmdQuery_am12 += " AND AM12.•”–å‚b‚c = '" + sKey[1] + "' \n";
				cmdQuery_am12 += " AND AM12.Œ³’…‹æ•ª = '" + sKey[2] + "' \n";
				cmdQuery_am12 += " AND AM12.íœ‚e‚f = '0' \n";
				cmdQuery_am12 += " FOR UPDATE \n";

				OracleDataReader reader_am12 = CmdSelect(sUser, conn2, cmdQuery_am12);
				int intCnt_am12 = 0;
				sRet[1] = "";
				if (reader_am12.Read())
				{
					i“o˜^˜A”Ô     = reader_am12.GetDecimal(0);
					iŠJnŒ´•[”Ô† = reader_am12.GetDecimal(1);
					iI—¹Œ´•[”Ô† = reader_am12.GetDecimal(2);
					iÅIŒ´•[”Ô† = reader_am12.GetDecimal(3);
					sŠ„•t“ú       = reader_am12.GetString(4).Trim();
					s—LŒøŠúŒÀ     = reader_am12.GetString(5).Trim();
					s“–“ú“ú•t     = reader_am12.GetString(6).Trim();
					intCnt_am12++;

					if (iÅIŒ´•[”Ô† < iI—¹Œ´•[”Ô† && int.Parse(s—LŒøŠúŒÀ) >= int.Parse(s“–“ú“ú•t))
					{
						//‘—‚èó”Ô†‚ÌƒZƒbƒg
						sRet[1] = (iÅIŒ´•[”Ô† + 1).ToString();
					}
				}
				disposeReader(reader_am12);
				reader_am12 = null;
				if (sRet[1].Length == 0)
				{
					//‚`‚l‚P‚Q‘—‚èóÌ”Ô‚ÉƒL[‚ª‘¶İ‚µ‚È‚¢A‚Ü‚½‚Í
					//ÅI”Ô† >= I—¹”Ô†A‚Ü‚½‚Í
					//—LŒøŠúŒÀ <  “–“ú‚Ì
					decimal iÅ‘å˜A”Ô   = 0;
					decimal iŠJn”Ô†   = 0;
					decimal iÅI”Ô†   = 0;
					decimal iI—¹”Ô†   = 0;
					decimal iŠ„•t–‡”   = 0;
					decimal i—LŒøŠúŒÀ   = 0;
					decimal i—LŒøŠúŒÀ”N = 0;
					decimal i—LŒøŠúŒÀŒ = 0;
					decimal i—LŒøŠúŒÀ“ú = 0;

					//Ì”ÔŠÇ—‚æ‚èV‹KŒ´•[”Ô†˜g‚ğæ“¾
					string cmdQuery_am10 = "SELECT";
					cmdQuery_am10 += " AM10.Å‘å˜A”Ô ";
					cmdQuery_am10 += ",AM10.“o˜^˜A”Ô ";
					cmdQuery_am10 += ",AM10.ÅIŒ´•[”Ô† ";
					cmdQuery_am10 += ",AM11.I—¹Œ´•[”Ô† ";
					cmdQuery_am10 += ",AM10.Š„•t–‡” ";
					cmdQuery_am10 += ",AM10.—LŒøŠúŒÀ ";
					cmdQuery_am10 += ",TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					cmdQuery_am10 += "FROM ‚`‚l‚P‚OÌ”ÔŠÇ— AM10 ";
					cmdQuery_am10 += ",‚`‚l‚P‚P‘—‚èó”Ô† AM11 \n";
					cmdQuery_am10 += " WHERE AM10.Ì”Ô‹æ•ª = '" + sKey[2] + "' \n";
					//cmdQuery_am10 += "   AND AM10.“o˜^˜A”Ô       =  " + i“o˜^˜A”Ô;
					cmdQuery_am10 += " AND AM10.Ì”Ô‹æ•ª = AM11.Œ³’…‹æ•ª \n";
					cmdQuery_am10 += " AND AM10.“o˜^˜A”Ô = AM11.“o˜^˜A”Ô \n";
					cmdQuery_am10 += " AND AM10.íœ‚e‚f = '0' \n";
					cmdQuery_am10 += " FOR UPDATE \n";

					OracleDataReader reader_am10 = CmdSelect(sUser, conn2, cmdQuery_am10);
					int intCnt_am10 = 0;
					if (reader_am10.Read())
					{
						iÅ‘å˜A”Ô     = reader_am10.GetDecimal(0);
						i“o˜^˜A”Ô     = reader_am10.GetDecimal(1);
						iÅI”Ô†     = reader_am10.GetDecimal(2);
						iI—¹”Ô†     = reader_am10.GetDecimal(3);
						iŠ„•t–‡”     = reader_am10.GetDecimal(4);
						i—LŒøŠúŒÀ     = reader_am10.GetDecimal(5);
						s“–“ú“ú•t     = reader_am10.GetString(6);

						//‘—‚èóÌ”ÔXVî•ñ‚Ìæ“¾
						iŠJnŒ´•[”Ô† = iÅI”Ô† + 1;
						iI—¹Œ´•[”Ô† = iÅI”Ô† + iŠ„•t–‡”;
						iÅIŒ´•[”Ô† = iŠJnŒ´•[”Ô†;
						sŠ„•t“ú       = s“–“ú“ú•t;
						i—LŒøŠúŒÀ”N   = int.Parse(sŠ„•t“ú.Substring(0, 4));
						i—LŒøŠúŒÀŒ   = int.Parse(sŠ„•t“ú.Substring(4, 2)) + i—LŒøŠúŒÀ - 1;
						if (i—LŒøŠúŒÀŒ > 12)
						{
							i—LŒøŠúŒÀ”N++;
							i—LŒøŠúŒÀŒ = i—LŒøŠúŒÀŒ - 12;
						}
						i—LŒøŠúŒÀ“ú   = System.DateTime.DaysInMonth(decimal.ToInt32(i—LŒøŠúŒÀ”N), decimal.ToInt32(i—LŒøŠúŒÀŒ));
						s—LŒøŠúŒÀ     = i—LŒøŠúŒÀ”N.ToString() + i—LŒøŠúŒÀŒ.ToString().PadLeft(2, '0') + i—LŒøŠúŒÀ“ú.ToString().PadLeft(2, '0');

						//Ì”ÔŠÇ—XVî•ñ‚Ìæ“¾
						iÅI”Ô†     = iI—¹Œ´•[”Ô†;

						sRet[1] = iÅIŒ´•[”Ô†.ToString();
						intCnt_am10++;
					}
					disposeReader(reader_am10);
					reader_am10 = null;
					if (intCnt_am10 == 0)
					{
						//ŠY“–ƒf[ƒ^‚ª‚È‚¢ê‡‚ÍƒGƒ‰[
						throw new Exception("ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ");
					}
					if (iÅI”Ô† > iI—¹”Ô†)
					{
						i“o˜^˜A”Ô++;
						if (i“o˜^˜A”Ô > iÅ‘å˜A”Ô)
						{
							i“o˜^˜A”Ô = 1;
						}
						//‘—‚èó”Ô†‚æ‚èV‹KŒ´•[”Ô†˜g‚ğæ“¾
						string cmdQuery_am11 = "SELECT";
						cmdQuery_am11 += " AM11.ŠJnŒ´•[”Ô† \n";
						cmdQuery_am11 += " FROM ‚`‚l‚P‚P‘—‚èó”Ô† AM11 \n";
						cmdQuery_am11 += " WHERE AM11.Œ³’…‹æ•ª = '" + sKey[2] + "' \n";
						cmdQuery_am11 += " AND AM11.“o˜^˜A”Ô =  " + i“o˜^˜A”Ô + " \n";
						cmdQuery_am11 += " AND AM11.íœ‚e‚f = '0' \n";
						cmdQuery_am11 += " FOR UPDATE \n";

						OracleDataReader reader_am11 = CmdSelect(sUser, conn2, cmdQuery_am11);
						int intCnt_am11 = 0;
						if (reader_am11.Read())
						{
							iŠJn”Ô†     = reader_am11.GetDecimal(0);
							//Ì”ÔŠÇ—XVî•ñ‚Ìæ“¾
							iÅI”Ô†     = iŠJn”Ô† + iŠ„•t–‡” - 1;
							//‘—‚èóÌ”ÔXVî•ñ‚Ìæ“¾
							iŠJnŒ´•[”Ô† = iŠJn”Ô†;
							iI—¹Œ´•[”Ô† = iÅI”Ô†;
							iÅIŒ´•[”Ô† = iŠJnŒ´•[”Ô†;

							sRet[1] = iÅIŒ´•[”Ô†.ToString();
							intCnt_am11++;
						}
						disposeReader(reader_am11);
						reader_am11 = null;
						if (intCnt_am11 == 0)
						{
							//ŠY“–ƒf[ƒ^‚ª‚È‚¢ê‡‚ÍƒGƒ‰[
							throw new Exception("ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ");
						}
					}
					// Ì”ÔŠÇ—‚ÌXV
					string updQuery_am10 = "UPDATE ‚`‚l‚P‚OÌ”ÔŠÇ— \n";
					updQuery_am10 += " SET “o˜^˜A”Ô = " + i“o˜^˜A”Ô;
					updQuery_am10 += ", ÅIŒ´•[”Ô† = " + iÅI”Ô†;
					updQuery_am10 += ", XV“ú = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "; // XV“ú
					updQuery_am10 += ", XVÒ = '" + sKey[3] + "' \n";                   // XVÒ
					updQuery_am10 += " WHERE Ì”Ô‹æ•ª = '" + sKey[2] + "' \n";

					CmdUpdate(sUser, conn2, updQuery_am10);
				}

				string updQuery_am12 = "";
				if (intCnt_am12 == 0)
				{
					// ‘—‚èóÌ”Ô‚Ì’Ç‰Á
					updQuery_am12  = "INSERT INTO ‚`‚l‚P‚Q‘—‚èóÌ”Ô \n";
					updQuery_am12 += " VALUES ('" + sKey[0] + "' ";
					updQuery_am12 +=         ",'" + sKey[1] + "' ";
					updQuery_am12 +=         ",'" + sKey[2] + "' ";
					updQuery_am12 +=         ", " + i“o˜^˜A”Ô;
					updQuery_am12 +=         ", " + iŠJnŒ´•[”Ô†;
					updQuery_am12 +=         ", " + iI—¹Œ´•[”Ô†;
					updQuery_am12 +=         ", " + iÅIŒ´•[”Ô†;
					updQuery_am12 +=         ",'" + sŠ„•t“ú + "' ";
					updQuery_am12 +=         ",'" + s—LŒøŠúŒÀ + "' ";
					updQuery_am12 +=         ",'0' ";
					updQuery_am12 +=         ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=         ",'o‰×“o˜^' ";
					updQuery_am12 +=         ",'" + sKey[3] + "' ";
					updQuery_am12 +=         ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=         ",'o‰×“o˜^' ";
					updQuery_am12 +=         ",'" + sKey[3] + "' ";
					updQuery_am12 += " ) ";
				}
				else
				{
					// ‘—‚èóÌ”Ô‚ÌXV
					updQuery_am12  = "UPDATE ‚`‚l‚P‚Q‘—‚èóÌ”Ô \n";
					updQuery_am12 += " SET “o˜^˜A”Ô =  " + i“o˜^˜A”Ô;
					updQuery_am12 +=      ", ŠJnŒ´•[”Ô† =  " + iŠJnŒ´•[”Ô†;
					updQuery_am12 +=      ", I—¹Œ´•[”Ô† =  " + iI—¹Œ´•[”Ô†;
					updQuery_am12 +=      ", ÅIŒ´•[”Ô† =  " + sRet[1];
					updQuery_am12 +=      ", Š„•t“ú = '" + sŠ„•t“ú + "'";
					updQuery_am12 +=      ", —LŒøŠúŒÀ = '" + s—LŒøŠúŒÀ + "'";
					updQuery_am12 +=      ", XV“ú =   TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=      ", XV‚o‚f = 'o‰×“o˜^' ";
					updQuery_am12 +=      ", XVÒ = '" + sKey[3] + "' \n";
					updQuery_am12 += " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";
					updQuery_am12 +=   " AND •”–å‚b‚c = '" + sKey[1] + "' \n";
					updQuery_am12 +=   " AND Œ³’…‹æ•ª = '" + sKey[2] + "' \n";
				}
				CmdUpdate(sUser, conn2, updQuery_am12);
				tran.Commit();
				sRet[0] = "³íI—¹";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * ‘—‚èó”Ô†XV
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA“o˜^“úAƒWƒƒ[ƒiƒ‹‚m‚nA‘—‚èó”Ô†AXVÒ
		 * @@@ˆóüƒf[ƒ^A—˜—pÒ•”–å“XŠ‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2print\Service1.asmx.cs(963):
		*/
		//		[WebMethod]
		private String[] Set_InvoiceNo2(string[] sUser, string[] sKey, string[] sPrintData, string sTensyo)
		{
			logWriter(sUser, INF, "‘—‚èó”Ô†XV‚QŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string s‘—‚èó”Ô† = "";
				sbQuery.Append("SELECT ‘—‚èó”Ô† \n");
				sbQuery.Append(" FROM  \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n");
				sbQuery.Append(" WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND •”–å‚b‚c = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND “o˜^“ú   = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND \"ƒWƒƒ[ƒiƒ‹‚m‚n\" = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND íœ‚e‚f = '0' \n");
				sbQuery.Append(" FOR UPDATE \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s‘—‚èó”Ô† = reader.GetString(0).TrimEnd();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				if(s‘—‚èó”Ô†.Length > 0)
				{
					tran.Commit();
					sRet[0] = "Ì”ÔÏ‚İ";
					sRet[1] = s‘—‚èó”Ô†;
					logWriter(sUser, INF, "‘—‚èó”Ô†XV‚Q@‘—‚èó”Ô†XVÏ["+s‘—‚èó”Ô†+"]");
					return sRet;
				}
				// ‘—‚èó”Ô†ƒ`ƒFƒbƒN
				String[] sGetKey = new string[4];
				sGetKey[0] = sKey[0];
				sGetKey[1] = sTensyo; // —˜—pÒ•”–å“XŠ‚b‚c
				sGetKey[2] = sPrintData[32]; //Œ³’…‹æ•ª + "0" or "1"
				if(sPrintData[14].Substring(1, 3) == "047")
				{
					sGetKey[2] = sPrintData[32].Substring(0,1) + "G"; //Œ³’…‹æ•ª + "G"
				}
				sGetKey[3] = sUser[1];
				String[] sGetData = this.Get_InvoiceNo(sUser, sGetKey);
				if(sGetData[0].Length != 4)
				{
					tran.Commit();
					sRet[0] = sGetData[0];
					return sRet;
				}
				//‘—‚èó”Ô†‚ÌƒZƒbƒg
				sPrintData[11] = sGetData[1].PadLeft(14, '0');
				//ƒ`ƒFƒbƒNƒfƒWƒbƒgi‚V‚ÅŠ„‚Á‚½—]‚èj‚Ì•t‰Á
				sPrintData[11] = sPrintData[11] + (long.Parse(sPrintData[11]) % 7).ToString();

				// o‰×ƒWƒƒ[ƒiƒ‹‚ÌXV
				string cmdQuery  = "UPDATE \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n";
				cmdQuery += " SET ‘—‚èó”Ô† = '"  + sPrintData[11] + "' ";                     // ‘—‚èó”Ô†
				cmdQuery += " WHERE ‰ïˆõ‚b‚c       = '" + sKey[0] + "' \n";
				cmdQuery +=   " AND •”–å‚b‚c       = '" + sKey[1] + "' \n";
				cmdQuery +=   " AND “o˜^“ú         = '" + sKey[2] + "' \n";
				cmdQuery +=   " AND ƒWƒƒ[ƒiƒ‹‚m‚n = '" + sKey[3] + "' \n";
				cmdQuery +=   " AND íœ‚e‚f       = '0' \n";

				CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "³íI—¹";
				logWriter(sUser, INF, "‘—‚èó”Ô†XV‚Q@‘—‚èó”Ô†XV["+sPrintData[11]+"]");
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
		// MOD 2011.03.25 “Œ“sj‚–Ø ‘—‚èó”Ô†‚Ìã‘‚«–h~ END

		/*********************************************************************
		 * ”­“Xæ“¾
		 * ˆø”F‰×‘—l‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA“XŠ‚b‚cA“XŠ–¼A“s“¹•{Œ§‚b‚cAs‹æ’¬‘º‚b‚cA‘åš’ÊÌ‚b‚c
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(1851):
		*/
		private String[] Get_hatuten(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[4];

			string cmdQuery = "SELECT Y.“XŠ‚b‚c, T.“XŠ–¼, Y.“s“¹•{Œ§‚b‚c, Y.s‹æ’¬‘º‚b‚c, Y.‘åš’ÊÌ‚b‚c \n"
				+ " FROM ‚b‚l‚O‚Q•”–å B, \n"
				+      " ‚b‚l‚P‚S—X•Ö”Ô†‚i Y, \n" // ‰¤q‰^‘—‘Î‰
				+      " ‚b‚l‚P‚O“XŠ T \n"
				+ " WHERE B.‰ïˆõ‚b‚c = '" + sKcode + "' \n"
				+ " AND B.•”–å‚b‚c = '" + sBcode + "' \n"
				+ " AND B.íœ‚e‚f = '0' \n"
				+ " AND B.—X•Ö”Ô† = Y.—X•Ö”Ô† \n"
				+ " AND Y.“XŠ‚b‚c = T.“XŠ‚b‚c \n"
				+ " AND T.íœ‚e‚f = '0' \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[1] = reader.GetString(0).Trim(); // “XŠ‚b‚c
				sRet[2] = reader.GetString(1).Trim(); // “XŠ–¼
				sRet[3] = reader.GetString(2).Trim()  // ZŠ‚b‚c
					+ reader.GetString(3).Trim()
					+ reader.GetString(4).Trim();

				sRet[0] = " ";
			}
			else
			{
				sRet[0] = "”­“X‚ğŒˆ‚ß‚ç‚ê‚Ü‚¹‚ñ‚Å‚µ‚½";
				sRet[1] = "0000";
				sRet[2] = " ";
				sRet[3] = " ";
			}
			disposeReader(reader);
			reader = null;
			
			return sRet;
		}

		/*********************************************************************
		 * ”­“Xæ“¾
		 * ˆø”F‰×‘—l‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA“XŠ‚b‚cA“XŠ–¼A“s“¹•{Œ§‚b‚cAs‹æ’¬‘º‚b‚cA‘åš’ÊÌ‚b‚c
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(1932):
		*/
		[WebMethod]
		public String[] Get_hatuten2(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "”­“Xæ“¾ŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery = "SELECT Y.“XŠ‚b‚c, T.“XŠ–¼, Y.“s“¹•{Œ§‚b‚c, Y.s‹æ’¬‘º‚b‚c, Y.‘åš’ÊÌ‚b‚c \n"
					+ " FROM ‚b‚l‚O‚Q•”–å B, \n"
					+      " ‚b‚l‚P‚S—X•Ö”Ô†‚i Y, \n" // ‰¤q‰^‘—‘Î‰
					+      " ‚b‚l‚P‚O“XŠ T \n"
					+ " WHERE B.‰ïˆõ‚b‚c = '" + sKcode + "' \n"
					+ " AND B.•”–å‚b‚c = '" + sBcode + "' \n"
					+ " AND B.íœ‚e‚f = '0' \n"
					+ " AND B.—X•Ö”Ô† = Y.—X•Ö”Ô† \n"
					+ " AND Y.“XŠ‚b‚c = T.“XŠ‚b‚c \n"
					+ " AND T.íœ‚e‚f = '0' \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				if(reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim()
						+ reader.GetString(3).Trim()
						+ reader.GetString(4).Trim();

					sRet[0] = "³íI—¹";
				}
				else
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				disposeReader(reader);
				reader = null;
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * W–ñ“Xæ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAW–ñ“X‚b‚c
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(2070):
		*/
		private String[] Get_syuuyakuten(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[2];

			string cmdQuery = "SELECT T.W–ñ“X‚b‚c \n"
				+ " FROM ‚b‚l‚O‚Q•”–å B,‚b‚l‚P‚O“XŠ T, \n"
				+        "‚b‚l‚P‚S—X•Ö”Ô†‚i Y  \n" // ‰¤q‰^‘—‘Î‰
				+ " WHERE B.‰ïˆõ‚b‚c   = '" + sKcode + "' \n"
				+ "   AND B.•”–å‚b‚c   = '" + sBcode + "' \n"
				+ "   AND B.íœ‚e‚f     = '0' \n"
				+    "AND B.—X•Ö”Ô† = Y.—X•Ö”Ô† \n"
				+    "AND Y.“XŠ‚b‚c     = T.“XŠ‚b‚c \n"
				+ "   AND T.íœ‚e‚f     = '0'";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0).Trim();
			}
			else
			{
				sRet[0] = "W–ñ“X‚ğŒˆ‚ß‚ç‚ê‚Ü‚¹‚ñ‚Å‚µ‚½";
				sRet[1] = "0000";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * W–ñ“Xæ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAW–ñ“X‚b‚c
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(2112):
		*/
		[WebMethod]
		public String[] Get_syuuyakuten2(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "W–ñ“Xæ“¾ŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery = "SELECT T.W–ñ“X‚b‚c \n"
					+ " FROM ‚b‚l‚O‚Q•”–å B,‚b‚l‚P‚O“XŠ T, \n"
					+        "‚b‚l‚P‚S—X•Ö”Ô†‚i Y  \n" // ‰¤q‰^‘—‘Î‰
					+ " WHERE B.‰ïˆõ‚b‚c   = '" + sKcode + "' \n"
					+ "   AND B.•”–å‚b‚c   = '" + sBcode + "' \n"
					+ "   AND B.íœ‚e‚f     = '0' \n"
					+    "AND B.—X•Ö”Ô† = Y.—X•Ö”Ô† \n"
					+    "AND Y.“XŠ‚b‚c     = T.“XŠ‚b‚c \n"
					+ "   AND T.íœ‚e‚f     = '0'";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				if(reader.Read())
				{
					sRet[0] = "³íI—¹";
					sRet[1] = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				disposeReader(reader);
				reader = null;
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}

		/*********************************************************************
		 * ’…“Xæ“¾
		 * @@‚r‚l‚O‚Q‰×ólA‚b‚l‚P‚S—X•Ö”Ô†A‚b‚l‚P‚T’…“X”ñ•\¦A‚b‚l‚P‚X—X•ÖZŠ
		 *     ‚Ì‚Sƒ}ƒXƒ^‚ğg—p‚µ‚Ä’…“XƒR[ƒh‚ğŒˆ’è‚·‚éB
		 * ˆø”F‰ïˆõƒR[ƒhA•”–åƒR[ƒhA‰×ólƒR[ƒhA—X•Ö”Ô†AZŠA–¼
		 * –ß’lFƒXƒe[ƒ^ƒXA“XŠ‚b‚cA“XŠ–¼AZŠ‚b‚c
		 *
		 * Create : 2008.06.12 kcl)X–{
		 * @@@@@@Get_tyakuten ‚ğŒ³‚Éì¬
		 * Modify : 2008.12.24 kcl)X–{
		 * @@@@@@‚b‚l‚P‚X‚ÌŒŸõ•û–@‚ğ•ÏXA‚¨‚æ‚Ñ–¼‚©‚ç‚ÌŒŸõ‚ğ’Ç‰Á
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(4769):
		*/
		private String[] Get_tyakuten3(string[] sUser, OracleConnection conn2, 
			string sKaiinCode, string sBumonCode, string sNiukeCode, 
			string sYuubin, string sJuusyo, string sShimei)
		{
			string [] sRet = new string [4];		// –ß‚è’l
			string cmdQuery;						// SQL•¶
			OracleDataReader reader;
			string tenCD       = string.Empty;		// “XŠƒR[ƒh
			string tenName     = string.Empty;		// “XŠ–¼
			string juusyoCD    = string.Empty;		// ZŠƒR[ƒh
			string address     = string.Empty;		// ZŠ
			string niuJuusyoCD = string.Empty;		// ‰×ólƒ}ƒXƒ^‚ÌZŠƒR[ƒh

			///
			/// ƒ‘æ‚P’iŠK„
			/// ‰×ólƒ}ƒXƒ^‚Ì’…“XƒR[ƒh‚ğŒŸõ
			/// 
			string niuCode = sNiukeCode.Trim();
			if (niuCode.Length > 0) 
			{
				// SQL•¶
				cmdQuery
					= "SELECT SM02.“Áê‚b‚c, NVL(CM10.“XŠ–¼, ' '), SM02.ZŠ‚b‚c \n"
					+ "  FROM ‚r‚l‚O‚Q‰×ól SM02 \n"
					+ "  LEFT OUTER JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
					+ "    ON SM02.“Áê‚b‚c   = CM10.“XŠ‚b‚c \n"
					+ "   AND CM10.íœ‚e‚f   = '0' \n"
					+ " WHERE SM02.‰ïˆõ‚b‚c   = '" + sKaiinCode + "' \n"
					+ "   AND SM02.•”–å‚b‚c   = '" + sBumonCode + "' \n"
					+ "   AND SM02.‰×ól‚b‚c = '" + sNiukeCode + "' \n"
					+ "   AND ( LENGTH(TRIM(SM02.“Áê‚b‚c)) > 0 \n"
					+ "      OR LENGTH(TRIM(SM02.ZŠ‚b‚c)) > 0 ) \n"
					+ "   AND SM02.íœ‚e‚f   = '0' \n";

				// SQLÀs
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// ƒf[ƒ^æ“¾
				if (reader.Read())
				{
					// ŠY“–ƒf[ƒ^‚ ‚è

					// ƒf[ƒ^æ“¾
					tenCD    = reader.GetString(0).Trim();		// “XŠƒR[ƒh
					tenName  = reader.GetString(1).Trim();		// “XŠ–¼
					juusyoCD = reader.GetString(2).Trim();		// ZŠƒR[ƒh

					if (tenCD.Length > 0) 
					{
						// ‰×ólƒ}ƒXƒ^‚Ì’…“XƒR[ƒh‚ª“ü—Í‚³‚ê‚Ä‚¢‚éê‡

						// ZŠƒR[ƒh‚Ìİ’è
						if (juusyoCD.Length == 0) 
						{
							// ‰×ólƒ}ƒXƒ^‚ÌZŠƒR[ƒh‚ª‹ó—“‚Ìê‡

							// —X•Ö”Ô†ƒ}ƒXƒ^‚©‚çæ“¾
							string [] sResult = this.Get_juusyoCode(sUser, conn2, sYuubin);
							if (sResult[0] == " ") 
								juusyoCD = sResult[1];
						}

						// –ß‚è’l‚ğƒZƒbƒg
						sRet[0] = " ";
						sRet[1] = tenCD;
						sRet[2] = tenName;
						sRet[3] = juusyoCD;

						// I—¹ˆ—
						disposeReader(reader);
						reader = null;
					
						return sRet;
					} 
					else
					{
						// ‰×ólƒ}ƒXƒ^‚ÉZŠƒR[ƒh‚Ì‚İ‚ª“ü—Í‚³‚ê‚Ä‚¢‚éê‡

						// ‰×ólƒ}ƒXƒ^‚ÌZŠƒR[ƒh‚ğ‚Æ‚Á‚Ä‚¨‚­
						niuJuusyoCD = juusyoCD;
					}
				}

				// I—¹ˆ—
				disposeReader(reader);
				reader = null;
			}

			///
			/// ƒ‘æ‚Q’iŠK„
			/// —X•Ö”Ô†ƒ}ƒXƒ^‚©‚ç’…“XƒR[ƒh‚ğŒŸõ
			///
			cmdQuery
				= "SELECT CM15.—X•Ö”Ô† \n"
				+ " FROM ‚b‚l‚P‚T’…“X”ñ•\¦‚i CM15 \n" // ‰¤q‰^‘—‘Î‰
				+ " WHERE CM15.—X•Ö”Ô† = '" + sYuubin + "' \n"
				+ "   AND CM15.íœ‚e‚f = '0' \n";

			// SQLÀs
			reader = CmdSelect(sUser, conn2, cmdQuery);
			// ƒf[ƒ^æ“¾
			if (reader.Read())
			{
				; // —X•Ö”Ô†ƒ}ƒXƒ^‚ÍŒŸõ‚µ‚È‚¢
			}
			else
			{
				// I—¹ˆ—
				disposeReader(reader);
				reader = null;
				// SQL•¶
				cmdQuery
					= "SELECT CM14.“XŠ‚b‚c, CM10.“XŠ–¼, CM14.“s“¹•{Œ§‚b‚c || CM14.s‹æ’¬‘º‚b‚c || CM14.‘åš’ÊÌ‚b‚c \n"
					+ "  FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i CM14 \n" // ‰¤q‰^‘—‘Î‰
					+ " INNER JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
					+ "    ON CM14.“XŠ‚b‚c = CM10.“XŠ‚b‚c \n"
					+ "   AND CM10.íœ‚e‚f = '0' \n"
					+ " WHERE CM14.—X•Ö”Ô† = '" + sYuubin + "' \n"
					+ "   AND LENGTH(TRIM(CM14.“XŠ‚b‚c)) > 0 \n"
					+ "   AND CM14.íœ‚e‚f = '0' \n";

				// SQLÀs
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// ƒf[ƒ^æ“¾
				if (reader.Read())
				{
					// ŠY“–ƒf[ƒ^‚ ‚è

					// ƒf[ƒ^æ“¾
					tenCD    = reader.GetString(0).Trim();		// “XŠƒR[ƒh
					tenName  = reader.GetString(1).Trim();		// “XŠ–¼
					juusyoCD = reader.GetString(2).Trim();		// ZŠƒR[ƒh

					// –ß‚è’l‚ğƒZƒbƒg
					sRet[0] = " ";
					sRet[1] = tenCD;
					sRet[2] = tenName;
					sRet[3] = (niuJuusyoCD.Length > 0) ? niuJuusyoCD : juusyoCD;
					// ªª ‰×ólƒ}ƒXƒ^‚ÌZŠƒR[ƒh‚ğ—Dæ‚·‚é

					// I—¹ˆ—
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
				else 
				{
					// ‚b‚l‚P‚S‚ÉŠY“–ƒf[ƒ^‚È‚µ

					// –ß‚è’l‚ğƒZƒbƒg
					sRet[0] = "“ü—Í‚³‚ê‚½‚¨“Í‚¯æ(—X•Ö”Ô†)‚Å‚Í”z’B“X‚ªŒˆ‚ß‚ç‚ê‚Ü‚¹‚ñ‚Å‚µ‚½";
					sRet[1] = "0000";
					sRet[2] = " ";
					sRet[3] = " ";

					// I—¹ˆ—
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
			}
			// I—¹ˆ—
			disposeReader(reader);
			reader = null;

			///
			/// ƒ‘æ‚R’iŠK„
			/// —X•ÖZŠƒ}ƒXƒ^‚©‚ç’…“XƒR[ƒh‚ğŒŸõ
			/// 
			// SQL•¶
			cmdQuery
				= "SELECT CM19.“XŠ‚b‚c, CM10.“XŠ–¼, CM19.ZŠ‚b‚c, CM19.ZŠ \n"
				+ "  FROM ‚b‚l‚P‚X—X•ÖZŠ‚i CM19 \n" // ‰¤q‰^‘—‘Î‰
				+ " INNER JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
				+ "    ON CM19.“XŠ‚b‚c = CM10.“XŠ‚b‚c \n"
				+ "   AND CM10.íœ‚e‚f = '0' \n"
				+ " WHERE CM19.—X•Ö”Ô† = '" + sYuubin + "' \n"
				+ "   AND CM19.íœ‚e‚f = '0' \n"
				+ " ORDER BY "
				+ "       LENGTH(TRIM(CM19.ZŠ)) DESC \n"
				;

			// SQLÀs
			reader = CmdSelect(sUser, conn2, cmdQuery);

			// ƒf[ƒ^æ“¾
			while (reader.Read()) 
			{
				// ZŠ‚Ìæ“¾
				address = reader.GetString(3).Trim();

				if (sShimei == null) sShimei = " ";

				// ZŠE–¼‚Ìƒ`ƒFƒbƒN
				if ((sJuusyo.IndexOf(address) >= 0) ||
					(sShimei.IndexOf(address) >= 0))
				{
					// ƒf[ƒ^æ“¾
					tenCD    = reader.GetString(0).Trim();	// “XŠƒR[ƒh
					tenName  = reader.GetString(1).Trim();	// “XŠ–¼
					juusyoCD = reader.GetString(2).Trim();	// ZŠƒR[ƒh

					// –ß‚è’l‚ğƒZƒbƒg
					sRet[0] = " ";
					sRet[1] = tenCD;
					sRet[2] = tenName;
					sRet[3] = (niuJuusyoCD.Length > 0) ? niuJuusyoCD : juusyoCD;
					// ªª ‰×ólƒ}ƒXƒ^‚ÌZŠƒR[ƒh‚ğ—Dæ‚·‚é

					// I—¹ˆ—
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
			}

			// I—¹ˆ—
			disposeReader(reader);
			reader = null;

			// ŠY“–ƒf[ƒ^–³
			sRet[0] = " ";
			sRet[1] = " ";
			sRet[2] = " ";
			sRet[3] = " ";
			
			return sRet;
		}

		/*********************************************************************
		 * ZŠƒR[ƒhæ“¾
		 * @@‚b‚l‚P‚S—X•Ö”Ô†‚ğg—p‚µ‚ÄA—X•Ö”Ô†‚©‚çZŠƒR[ƒh‚ğæ“¾‚·‚éB
		 * ˆø”F—X•Ö”Ô†
		 * –ß’lFƒXƒe[ƒ^ƒXAZŠ‚b‚c
		 *
		 * Create : 2008.06.16 kcl)X–{
		 * @@@@@@V‹Kì¬
		 * Modify : 
		 *********************************************************************/
		private String[] Get_juusyoCode(string[] sUser, OracleConnection conn2, 
			string sYuubin)
		{
			string [] sRet = new string [2];	// –ß‚è’l
			string cmdQuery;					// SQL•¶
			OracleDataReader reader;

			// SQL•¶
			cmdQuery
				= "SELECT CM14.“s“¹•{Œ§‚b‚c || CM14.s‹æ’¬‘º‚b‚c || CM14.‘åš’ÊÌ‚b‚c \n"
				+ "  FROM ‚b‚l‚P‚S—X•Ö”Ô† CM14 \n"
				+ " WHERE CM14.—X•Ö”Ô† = '" + sYuubin + "' \n"
				+ "   AND CM14.íœ‚e‚f = '0' \n";

			// SQLÀs
			reader = CmdSelect(sUser, conn2, cmdQuery);

			// ƒf[ƒ^æ“¾
			if (reader.Read())
			{
				// ŠY“–ƒf[ƒ^‚ ‚è
				sRet[0] = " ";							// ƒXƒe[ƒ^ƒX
				sRet[1] = reader.GetString(0).Trim();	// ZŠƒR[ƒh
			} 
			else
			{
				// ŠY“–ƒf[ƒ^–³
				sRet[0] = "“ü—Í‚³‚ê‚½—X•Ö”Ô†‚Å‚ÍZŠƒR[ƒh‚ªŒˆ‚ß‚ç‚ê‚Ü‚¹‚ñ‚Å‚µ‚½";
				sRet[1] = " ";
			}

			// I—¹ˆ—
			disposeReader(reader);
			reader = null;
			
			return sRet;
		}

		/*********************************************************************
		 * o‰×ƒf[ƒ^XV
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cAo‰×“ú...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(1161):
		*/
		[WebMethod]
		public String[] Upd_syukka2(string[] sUser, string[] sData, string sNo)
		{
			logWriter(sUser, INF, "o‰×XVŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			decimal dŒ”;
			string s“ÁêŒv = " ";
			try
			{
				//o‰×“úƒ`ƒFƒbƒN
				string[] sSyukkabi = Get_bumonsyukka(sUser, conn2, sData[0], sData[1]);
				sRet[0] = sSyukkabi[0];
				if(sRet[0] != " ") return sRet;
				if(int.Parse(sData[2]) < int.Parse(sSyukkabi[1]))
				{
					sRet[0] = "1";
					sRet[1] = sSyukkabi[1];
					return sRet;
				}

				//‰×‘—l‚b‚c‘¶İƒ`ƒFƒbƒN
				string cmdQuery
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					//					= "SELECT “¾ˆÓæ‚b‚c, “¾ˆÓæ•”‰Û‚b‚c \n"
					//					+ "  FROM ‚r‚l‚O‚P‰×‘—l \n"
					//					+ " WHERE ‰ïˆõ‚b‚c   = '" + sData[0]  +"' \n"
					//					+ "   AND •”–å‚b‚c   = '" + sData[1]  +"' \n"
					//					+ "   AND ‰×‘—l‚b‚c = '" + sData[15] +"' \n"
					//					+ "   AND íœ‚e‚f   = '0'";
					= "SELECT SM01.“¾ˆÓæ‚b‚c, SM01.“¾ˆÓæ•”‰Û‚b‚c \n"
					+ "     , NVL(CM01.•Û—¯ˆóü‚e‚f,'0') \n"
					+ "  FROM ‚r‚l‚O‚P‰×‘—l SM01 \n"
					+ "     , ‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+ " WHERE SM01.‰ïˆõ‚b‚c   = '" + sData[0]  +"' \n"
					+ "   AND SM01.•”–å‚b‚c   = '" + sData[1]  +"' \n"
					+ "   AND SM01.‰×‘—l‚b‚c = '" + sData[15] +"' \n"
					+ "   AND SM01.íœ‚e‚f   = '0' \n"
					+ "   AND SM01.‰ïˆõ‚b‚c   = CM01.‰ïˆõ‚b‚c(+) \n"
					;
				string sd—Ê“ü—Í§Œä = "0";
				// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if(reader.Read())
				{
					dŒ” = 1;
					sData[16] = reader.GetString(0);
					sData[17] = reader.GetString(1);
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					sd—Ê“ü—Í§Œä = reader.GetString(2);
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
				}
				else
				{
					dŒ” = 0;
				}
				disposeReader(reader);
				reader = null;

				if(dŒ” == 0)
				{
					sRet[0] = "0";
				}
				else
				{
					cmdQuery
						= "SELECT SM04.“¾ˆÓæ•”‰Û–¼ \n"
						+ " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
						+    " , ‚r‚l‚O‚S¿‹æ SM04 \n"
						+ " WHERE CM02.‰ïˆõ‚b‚c = '" + sData[0] + "' \n"
						+  " AND CM02.•”–å‚b‚c = '" + sData[1] + "' \n"
						+  " AND CM02.íœ‚e‚f = '0' \n"
						+  " AND SM04.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
						+  " AND SM04.—X•Ö”Ô† = CM02.—X•Ö”Ô† \n"
						+  " AND SM04.“¾ˆÓæ‚b‚c = '" + sData[16] + "' \n"
						+  " AND SM04.“¾ˆÓæ•”‰Û‚b‚c = '" + sData[17] + "' \n"
						+  " AND SM04.íœ‚e‚f = '0' \n"
						;
					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(reader.Read())
					{
						sData[18] = reader.GetString(0);
					}
					else
					{
						sData[18] = " ";
					}
					disposeReader(reader);
					reader = null;

					//“ÁêŒvæ“¾
					if(sData[4] != " ")
					{
						cmdQuery
							= "SELECT NVL(“ÁêŒv,' ') \n"
							+ "  FROM ‚r‚l‚O‚Q‰×ól \n"
							+ " WHERE ‰ïˆõ‚b‚c   = '" + sData[0] +"' \n"
							+ "   AND •”–å‚b‚c   = '" + sData[1] +"' \n"
							+ "   AND ‰×ól‚b‚c = '" + sData[4] +"' \n"
							+ "   AND íœ‚e‚f   = '0'";

						reader = CmdSelect(sUser, conn2, cmdQuery);

						bool bRead = reader.Read();
						if(bRead == true)
							s“ÁêŒv   = reader.GetString(0);

						disposeReader(reader);
						reader = null;
						cmdQuery
							= "UPDATE ‚r‚l‚O‚Q‰×ól \n"
							+ " SET “o˜^‚o‚f = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ " WHERE ‰ïˆõ‚b‚c = '" + sData[0] +"' \n"
							+ " AND •”–å‚b‚c   = '" + sData[1] +"' \n"
							+ " AND ‰×ól‚b‚c = '" + sData[4] +"' \n"
							+ " AND íœ‚e‚f   = '0'";
						try
						{
							int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
						}
						catch(Exception)
						{
							;
						}
					}

					//’…“Xæ“¾
					string[] sTyakuten = Get_tyakuten3(sUser, conn2, 
						sData[0], sData[1], sData[4], 
						sData[13] + sData[14], sData[8] + sData[9] + sData[10], sData[11] + sData[12]);
					sRet[0] = sTyakuten[0];
					if(sRet[0] != " ") return sRet;
					string s’…“X‚b‚c = (sTyakuten[1].Length > 0) ? sTyakuten[1] : " ";
					string s’…“X–¼   = (sTyakuten[2].Length > 0) ? sTyakuten[2] : " ";
					string sZŠ‚b‚c = (sTyakuten[3].Length > 0) ? sTyakuten[3] : " ";

					//”­“Xæ“¾
					string[] sHatuten = Get_hatuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sHatuten[0];
					if(sRet[0] != " ") return sRet;
					string s”­“X‚b‚c = sHatuten[1];
					string s”­“X–¼   = sHatuten[2];

					//W‰×“Xæ“¾
					string[] sSyuyaku = Get_syuuyakuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sSyuyaku[0];
					if(sRet[0] != " ") return sRet;
					string sW–ñ“X‚b‚c = sSyuyaku[1];

					//d•ª‚b‚cæ“¾
					string sd•ª‚b‚c = " ";
					if(s”­“X‚b‚c.Trim().Length > 0 && s’…“X‚b‚c.Trim().Length > 0)
					{
						string[] sRetSiwake = Get_siwake(sUser, conn2, s”­“X‚b‚c, s’…“X‚b‚c);
						sd•ª‚b‚c = sRetSiwake[1];
					}
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					if(sd—Ê“ü—Í§Œä == "0")
					{
						sData[38] = "0"; // Ë”
						sData[20] = "0"; // d—Ê
					}
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
					string s•i–¼‹L–‚S = (sData.Length > 43) ? sData[43] : " ";
					string s•i–¼‹L–‚T = (sData.Length > 44) ? sData[44] : " ";
					string s•i–¼‹L–‚U = (sData.Length > 45) ? sData[45] : " ";
					if(s•i–¼‹L–‚S.Length == 0) s•i–¼‹L–‚S = " ";
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END

					cmdQuery 
						= "UPDATE \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n"
						+    "SET o‰×“ú             = '" + sData[2]  +"', \n"
						+        "‚¨‹q—lo‰×”Ô†     = '" + sData[3]  +"',"
						+        "‰×ól‚b‚c         = '" + sData[4]  +"',"
						+        "“d˜b”Ô†‚P         = '" + sData[5]  +"', \n"
						+        "“d˜b”Ô†‚Q         = '" + sData[6]  +"',"
						+        "“d˜b”Ô†‚R         = '" + sData[7]  +"',"
						+        "ZŠ‚b‚c           = '" + sZŠ‚b‚c +"', \n"
						+        "ZŠ‚P             = '" + sData[8]  +"',"
						+        "ZŠ‚Q             = '" + sData[9]  +"',"
						+        "ZŠ‚R             = '" + sData[10] +"', \n"
						+        "–¼‘O‚P             = '" + sData[11] +"',"
						+        "–¼‘O‚Q             = '" + sData[12] +"',"
						+        "—X•Ö”Ô†           = '" + sData[13] + sData[14] +"', \n"
						+        "’…“X‚b‚c           = '" + s’…“X‚b‚c +"',"
						+        "’…“X–¼             = '" + s’…“X–¼   +"',"
						+        "“ÁêŒv             = '" + s“ÁêŒv   +"', \n"
						+        "‰×‘—l‚b‚c         = '" + sData[15] +"',"
						+        "‰×‘—l•”–¼       = '" + sData[37] +"',"
						+        "W–ñ“X‚b‚c         = '" + sW–ñ“X‚b‚c +"', \n"
						+        "”­“X‚b‚c           = '" + s”­“X‚b‚c +"',"
						+        "”­“X–¼             = '" + s”­“X–¼   +"',"
						+        "“¾ˆÓæ‚b‚c         = '" + sData[16] +"', \n"
						+        "•”‰Û‚b‚c           = '" + sData[17] +"',"
						+        "•”‰Û–¼             = '" + sData[18] +"',"
						+        "ŒÂ”               =  " + sData[19] +", \n"
						+        "Ë”               =  " + sData[38] +","
						+        "d—Ê               =  " + sData[20] +","
						+        "w’è“ú             = '" + sData[21] +"',"
						+        "w’è“ú‹æ•ª         = '" + sData[41] +"',"
						+        "—A‘—w¦‚b‚c‚P     = '" + sData[39] +"',"
						+        "—A‘—w¦‚P         = '" + sData[22] +"', \n"
						+        "—A‘—w¦‚b‚c‚Q     = '" + sData[40] +"',"
						+        "—A‘—w¦‚Q         = '" + sData[23] +"',"
						+        "•i–¼‹L–‚P         = '" + sData[24] +"',"
						+        "•i–¼‹L–‚Q         = '" + sData[25] +"', \n"
						+        "•i–¼‹L–‚R         = '" + sData[26] +"',"
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
						+        "•i–¼‹L–‚S         = '" + s•i–¼‹L–‚S +"', \n"
						+        "•i–¼‹L–‚T         = '" + s•i–¼‹L–‚T +"',"
						+        "•i–¼‹L–‚U         = '" + s•i–¼‹L–‚U +"', \n"
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
						+        "•ÛŒ¯‹àŠz           =  " + sData[28] +","
						+        "d•ª‚b‚c           = '" + sd•ª‚b‚c + "', \n"
						+        "‘—‚èó”­sÏ‚e‚f   = '0', \n"
						+        "‘—MÏ‚e‚f         = '0',"
						+        "ó‘Ô               = '01',"
						+        "Ú×ó‘Ô           = '  ', \n"
						+        "XV‚o‚f           = '" + sData[32] +"',"
						+        "XVÒ             = '" + sData[33] +"', \n"
						+        "XV“ú           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE ‰ïˆõ‚b‚c           = '" + sData[0]  +"' \n"
						+ "   AND •”–å‚b‚c           = '" + sData[1]  +"' \n"
						+ "   AND “o˜^“ú             = '" + sData[35] +"' \n"
						+ "   AND \"ƒWƒƒ[ƒiƒ‹‚m‚n\" = '" + sData[34] +"' \n"
						+ "   AND XV“ú           =  " + sData[36] +"";
					logWriter(sUser, INF, "o‰×XV["+sData[1]+"]["+sData[35]+"]["+sData[34]+"]:["+sNo+"]");

					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					tran.Commit();
					if(iUpdRow == 0)
						sRet[0] = "ƒf[ƒ^•ÒW’†‚É‘¼‚Ì’[––‚æ‚èXV‚³‚ê‚Ä‚¢‚Ü‚·B\r\nÄ“xAÅVƒf[ƒ^‚ğŒÄ‚Ño‚µ‚ÄXV‚µ‚Ä‚­‚¾‚³‚¢B";
					else
						sRet[0] = "³íI—¹";
				}
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * o‰×ƒf[ƒ^“o˜^
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cAo‰×“ú...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(736):
		*/
		[WebMethod]
		public String[] Ins_syukka(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "o‰×“o˜^ŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			decimal dŒ”;
			string s“ÁêŒv = " ";
			string s“o˜^“ú;
			int iŠÇ—‚m‚n;
			string s“ú•t;
			try
			{
				//o‰×“úƒ`ƒFƒbƒN
				string[] sSyukkabi = Get_bumonsyukka(sUser, conn2, sData[0], sData[1]);
				sRet[0] = sSyukkabi[0];
				if(sRet[0] != " ") return sRet;
				if(int.Parse(sData[2]) < int.Parse(sSyukkabi[1]))
				{
					sRet[0] = "1";
					sRet[1] = sSyukkabi[1];
					return sRet;
				}

				//‰×‘—l‚b‚c‘¶İƒ`ƒFƒbƒN
				string cmdQuery
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					//					= "SELECT “¾ˆÓæ‚b‚c, “¾ˆÓæ•”‰Û‚b‚c \n"
					//					+ "  FROM ‚r‚l‚O‚P‰×‘—l \n"
					//					+ " WHERE ‰ïˆõ‚b‚c   = '" + sData[0]  +"' \n"
					//					+ "   AND •”–å‚b‚c   = '" + sData[1]  +"' \n"
					//					+ "   AND ‰×‘—l‚b‚c = '" + sData[15] +"' \n"
					//					+ "   AND íœ‚e‚f   = '0'";
					= "SELECT SM01.“¾ˆÓæ‚b‚c, SM01.“¾ˆÓæ•”‰Û‚b‚c \n"
					+ "     , NVL(CM01.•Û—¯ˆóü‚e‚f,'0') \n"
					+ "  FROM ‚r‚l‚O‚P‰×‘—l SM01 \n"
					+ "     , ‚b‚l‚O‚P‰ïˆõ CM01 \n"
					+ " WHERE SM01.‰ïˆõ‚b‚c   = '" + sData[0]  +"' \n"
					+ "   AND SM01.•”–å‚b‚c   = '" + sData[1]  +"' \n"
					+ "   AND SM01.‰×‘—l‚b‚c = '" + sData[15] +"' \n"
					+ "   AND SM01.íœ‚e‚f   = '0' \n"
					+ "   AND SM01.‰ïˆõ‚b‚c   = CM01.‰ïˆõ‚b‚c(+) \n"
					;
				string sd—Ê“ü—Í§Œä = "0";
				// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if(reader.Read())
				{
					dŒ” = 1;
					sData[16] = reader.GetString(0);
					sData[17] = reader.GetString(1);
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					sd—Ê“ü—Í§Œä = reader.GetString(2);
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
				}
				else
				{
					dŒ” = 0;
				}
				disposeReader(reader);
				reader = null;
				if(dŒ” == 0)
				{
					sRet[0] = "0";
				}
				else
				{
					cmdQuery
						= "SELECT SM04.“¾ˆÓæ•”‰Û–¼ \n"
						+ " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
						+    " , ‚r‚l‚O‚S¿‹æ SM04 \n"
						+ " WHERE CM02.‰ïˆõ‚b‚c = '" + sData[0] + "' \n"
						+  " AND CM02.•”–å‚b‚c = '" + sData[1] + "' \n"
						+  " AND CM02.íœ‚e‚f = '0' \n"
						+  " AND SM04.—X•Ö”Ô† = CM02.—X•Ö”Ô† \n"
						+  " AND SM04.“¾ˆÓæ‚b‚c = '" + sData[16] + "' \n"
						+  " AND SM04.“¾ˆÓæ•”‰Û‚b‚c = '" + sData[17] + "' \n"
						// MOD 2011.03.09 “Œ“sj‚–Ø ¿‹æƒ}ƒXƒ^‚ÌåƒL[‚É[‰ïˆõ‚b‚c]‚ğ’Ç‰Á START
						+  " AND SM04.‰ïˆõ‚b‚c = CM02.‰ïˆõ‚b‚c \n"
						// MOD 2011.03.09 “Œ“sj‚–Ø ¿‹æƒ}ƒXƒ^‚ÌåƒL[‚É[‰ïˆõ‚b‚c]‚ğ’Ç‰Á END
						+  " AND SM04.íœ‚e‚f = '0' \n"
						;
					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(reader.Read())
					{
						sData[18] = reader.GetString(0);
					}
					else
					{
						sData[18] = " ";
					}
					disposeReader(reader);
					reader = null;

					//“ÁêŒvæ“¾
					if(sData[4] != " ")
					{
						cmdQuery
							= "SELECT NVL(“ÁêŒv,' ') \n"
							+ "  FROM ‚r‚l‚O‚Q‰×ól \n"
							+ " WHERE ‰ïˆõ‚b‚c   = '" + sData[0] +"' \n"
							+ "   AND •”–å‚b‚c   = '" + sData[1] +"' \n"
							+ "   AND ‰×ól‚b‚c = '" + sData[4] +"' \n"
							+ "   AND íœ‚e‚f   = '0'";

						reader = CmdSelect(sUser, conn2, cmdQuery);
						bool bRead = reader.Read();
						if(bRead == true)
							s“ÁêŒv   = reader.GetString(0);

						disposeReader(reader);
						reader = null;
						cmdQuery
							= "UPDATE ‚r‚l‚O‚Q‰×ól \n"
							+ " SET “o˜^‚o‚f = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ " WHERE ‰ïˆõ‚b‚c = '" + sData[0] +"' \n"
							+ " AND •”–å‚b‚c   = '" + sData[1] +"' \n"
							+ " AND ‰×ól‚b‚c = '" + sData[4] +"' \n"
							+ " AND íœ‚e‚f   = '0'";
						try
						{
							int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
						}
						catch(Exception)
						{
							;
						}
					}

					//ƒWƒƒ[ƒiƒ‹‚m‚næ“¾
					cmdQuery
						= "SELECT \"ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú\",\"ƒWƒƒ[ƒiƒ‹‚m‚nŠÇ—\", \n"
						+ "       TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+ "  FROM ‚b‚l‚O‚Q•”–å \n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sData[0] +"' \n"
						+ "   AND •”–å‚b‚c = '" + sData[1] +"' \n"
						+ "   AND íœ‚e‚f = '0'"
						+ "   FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					reader.Read();
					s“o˜^“ú   = reader.GetString(0).Trim();
					iŠÇ—‚m‚n = reader.GetInt32(1);
					s“ú•t     = reader.GetString(2);

					if(s“o˜^“ú == s“ú•t)
						iŠÇ—‚m‚n++;
					else
					{
						s“o˜^“ú = s“ú•t;
						iŠÇ—‚m‚n = 1;
					}

					cmdQuery 
						= "UPDATE ‚b‚l‚O‚Q•”–å \n"
						+    "SET \"ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú\"  = '" + s“o˜^“ú +"', \n"
						+        "\"ƒWƒƒ[ƒiƒ‹‚m‚nŠÇ—\"    = " + iŠÇ—‚m‚n +", \n"
						+        "XV‚o‚f                  = '" + sData[32] +"', \n"
						+        "XVÒ                    = '" + sData[33] +"', \n"
						+        "XV“ú                  =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE ‰ïˆõ‚b‚c       = '" + sData[0] +"' \n"
						+ "   AND •”–å‚b‚c       = '" + sData[1] +"' \n"
						+ "   AND íœ‚e‚f = '0'";

					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					disposeReader(reader);
					reader = null;

					//’…“Xæ“¾
					string[] sTyakuten = Get_tyakuten3(sUser, conn2, 
						sData[0], sData[1], sData[4], 
						sData[13] + sData[14], sData[8] + sData[9] + sData[10], sData[11] + sData[12]);
					sRet[0] = sTyakuten[0];
					if(sRet[0] != " ") return sRet;
					string s’…“X‚b‚c = (sTyakuten[1].Length > 0) ? sTyakuten[1] : " ";
					string s’…“X–¼   = (sTyakuten[2].Length > 0) ? sTyakuten[2] : " ";
					string sZŠ‚b‚c = (sTyakuten[3].Length > 0) ? sTyakuten[3] : " ";

					//”­“Xæ“¾
					string[] sHatuten = Get_hatuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sHatuten[0];
					if(sRet[0] != " ") return sRet;
					string s”­“X‚b‚c = sHatuten[1];
					string s”­“X–¼   = sHatuten[2];

					//W‰×“Xæ“¾
					string[] sSyuyaku = Get_syuuyakuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sSyuyaku[0];
					if(sRet[0] != " ") return sRet;
					string sW–ñ“X‚b‚c = sSyuyaku[1];

					//d•ª‚b‚cæ“¾
					string sd•ª‚b‚c = " ";
					if(s”­“X‚b‚c.Trim().Length > 0 && s’…“X‚b‚c.Trim().Length > 0)
					{
						string[] sRetSiwake = Get_siwake(sUser, conn2, s”­“X‚b‚c, s’…“X‚b‚c);
						sd•ª‚b‚c = sRetSiwake[1];
					}

					// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ START
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
					//					// ˆ—‚O‚Q‚ÉË”‚¨‚æ‚Ñd—Ê‚ÌQl’l‚ğ“ü‚ê‚é
					//					string sË” = "";
					//					string sd—Ê = "";
					//					string sË”d—Ê = "";
					//					try{
					//						sË” = sData[38].Trim().PadLeft(5,'0');
					//						sd—Ê = sData[20].Trim().PadLeft(5,'0');
					//						sË”d—Ê = sË”.Substring(0,5)
					//									+ sd—Ê.Substring(0,5);
					//					}catch(Exception){
					//					}
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					///					string sd—Ê“ü—Í§Œä = (sData.Length > 42) ? sData[42] : "0";
					///					if(sd—Ê“ü—Í§Œä != "1"){
					///					string sd—Ê“ü—Í§Œä = (sData.Length > 42) ? sData[42] : " ";
					if(sd—Ê“ü—Í§Œä == "0")
					{
						// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
						sData[38] = "0"; // Ë”
						sData[20] = "0"; // d—Ê
						// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
					}
					// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
					// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ END
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
					string s•i–¼‹L–‚S = (sData.Length > 43) ? sData[43] : " ";
					string s•i–¼‹L–‚T = (sData.Length > 44) ? sData[44] : " ";
					string s•i–¼‹L–‚U = (sData.Length > 45) ? sData[45] : " ";
					if(s•i–¼‹L–‚S.Length == 0) s•i–¼‹L–‚S = " ";
					// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
					cmdQuery 
						= "INSERT INTO \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n"
						+ "(‰ïˆõ‚b‚c, •”–å‚b‚c, “o˜^“ú, \"ƒWƒƒ[ƒiƒ‹‚m‚n\", o‰×“ú \n"
						+ ", ‚¨‹q—lo‰×”Ô†, ‰×ól‚b‚c \n"
						+ ", “d˜b”Ô†‚P, “d˜b”Ô†‚Q, “d˜b”Ô†‚R, ‚e‚`‚w”Ô†‚P, ‚e‚`‚w”Ô†‚Q, ‚e‚`‚w”Ô†‚R \n"
						+ ", ZŠ‚b‚c, ZŠ‚P, ZŠ‚Q, ZŠ‚R \n"
						+ ", –¼‘O‚P, –¼‘O‚Q, –¼‘O‚R \n"
						+ ", —X•Ö”Ô† \n"
						+ ", ’…“X‚b‚c, ’…“X–¼, “ÁêŒv \n"
						+ ", ‰×‘—l‚b‚c, ‰×‘—l•”–¼ \n"
						+ ", W–ñ“X‚b‚c, ”­“X‚b‚c, ”­“X–¼ \n"
						+ ", “¾ˆÓæ‚b‚c, •”‰Û‚b‚c, •”‰Û–¼ \n"
						+ ", ŒÂ”, Ë”, d—Ê, ƒ†ƒjƒbƒg \n"
						+ ", w’è“ú, w’è“ú‹æ•ª \n"
						+ ", —A‘—w¦‚b‚c‚P, —A‘—w¦‚P \n"
						+ ", —A‘—w¦‚b‚c‚Q, —A‘—w¦‚Q \n"
						+ ", •i–¼‹L–‚P, •i–¼‹L–‚Q, •i–¼‹L–‚R \n"
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
						+ ", •i–¼‹L–‚S, •i–¼‹L–‚T, •i–¼‹L–‚U \n"
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
						+ ", Œ³’…‹æ•ª, •ÛŒ¯‹àŠz, ‰^’À, ’†Œp, ”—¿‹à \n"
						+ ", d•ª‚b‚c, ‘—‚èó”Ô†, ‘—‚èó‹æ•ª \n"
						+ ", ‘—‚èó”­sÏ‚e‚f, o‰×Ï‚e‚f, ‘—MÏ‚e‚f, ˆêŠ‡o‰×‚e‚f \n"
						+ ", ó‘Ô, Ú×ó‘Ô \n"
						// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ START
						+ ", ˆ—‚O‚Q \n"
						// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ END
						+ ", íœ‚e‚f, “o˜^“ú, “o˜^‚o‚f, “o˜^Ò \n"
						+ ", XV“ú, XV‚o‚f, XVÒ \n"
						+ ") \n"
						+ "VALUES ('" + sData[0] +"','" + sData[1] +"','" + s“ú•t +"'," + iŠÇ—‚m‚n +",'" + sData[2] +"', \n"
						+         "'" + sData[3] +"','" + sData[4] +"', \n"
						+         "'" + sData[5] +"','" + sData[6] +"','" + sData[7] +"',' ',' ',' ', \n"
						+         "'" + sZŠ‚b‚c +"','" + sData[8] +"','" + sData[9] +"','" + sData[10] +"', \n"
						+         "'" + sData[11] +"','" + sData[12] +"',' ', \n"
						+         "'" + sData[13] + sData[14] +"', \n"
						+         "'" + s’…“X‚b‚c +"','" + s’…“X–¼ + "','" + s“ÁêŒv +"', \n"        //’…“X‚b‚c@’…“X–¼@“ÁêŒv
						+         "'" + sData[15] +"','" + sData[37] +"', \n"						  // ‰×‘—l‚b‚c  ‰×‘—l•”–¼
						+         "'" + sW–ñ“X‚b‚c + "','" + s”­“X‚b‚c + "','" + s”­“X–¼ + "', \n"  //W–ñ“X‚b‚c@”­“X‚b‚c@”­“X–¼
						+         "'" + sData[16] +"','" + sData[17] +"','" + sData[18] +"', \n"
						+         "" + sData[19] +"," + sData[38] +"," + sData[20] +",0, \n"
						+         "'" + sData[21] +"','" + sData[41] +"', \n"
						+         "'" + sData[39] +"','" + sData[22] +"', \n"
						+         "'" + sData[40] +"','" + sData[23] +"', \n"
						+         "'" + sData[24] +"','" + sData[25] +"','" + sData[26] +"', \n"
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
						+         "'" + s•i–¼‹L–‚S +"','"+ s•i–¼‹L–‚T +"','"+ s•i–¼‹L–‚U +"', \n"
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
						+         "'" + sData[27] +"'," + sData[28] +",0,0,0, \n"  //‰^’À@’†Œp@”—¿‹à
						+         "'" + sd•ª‚b‚c + "',' ',' ',"  //  d•ª‚b‚c  ‘—‚èó”Ô†  ‘—‚èó‹æ•ª
						+         "'" + sData[29] +"','" + sData[30] +"', '0', '" + sData[31] +"', \n"  //   ‘—MÏ‚e‚f
						+         "'01','  ', \n"        //ó‘Ô@Ú×ó‘Ô
						// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ START
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á START
						//						+         "'" + sË”d—Ê + "', \n" // ˆ—‚O‚Q
						+         "' ', \n" // ˆ—‚O‚Q
						// MOD 2011.07.14 “Œ“sj‚–Ø ‹L–s‚Ì’Ç‰Á END
						// MOD 2011.04.13 “Œ“sj‚–Ø d—Ê“ü—Í•s‰Â‘Î‰ END
						+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[32] +"','" + sData[33] +"', \n"
						+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[32] +"','" + sData[33] +"')";
					logWriter(sUser, INF, "o‰×“o˜^["+sData[1]+"]["+s“ú•t+"]["+iŠÇ—‚m‚n+"]");

					iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					tran.Commit();
					sRet[0] = "³íI—¹";
					sRet[1] = s“ú•t;
					sRet[2] = iŠÇ—‚m‚n.ToString();
				}

			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
				if(ex.Number == 1438)
				{ // ORA-01438: value larger than specified precision allows for this column
					//					if(iŠÇ—‚m‚n > 9999){
					sRet[0] = "‚P“ú‚Åˆµ‚¦‚éo‰×”i9999Œj‚ğ‰z‚¦‚Ü‚µ‚½B";
					//					}
				}
			}
			catch (Exception ex)
			{
				tran.Rollback();
				string sErr = ex.Message.Substring(0,9);
				if(sErr == "ORA-00001")
					sRet[0] = "“¯ˆê‚ÌƒR[ƒh‚ªŠù‚É‘¼‚Ì’[––‚æ‚è“o˜^‚³‚ê‚Ä‚¢‚Ü‚·B\r\nÄ“xAÅVƒf[ƒ^‚ğŒÄ‚Ño‚µ‚ÄXV‚µ‚Ä‚­‚¾‚³‚¢B";
				else
					sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * •”–åƒ}ƒXƒ^o‰×“úæ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAo‰×“ú
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(2246):
		*/
		private String[] Get_bumonsyukka(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[2];

			string cmdQuery = "SELECT o‰×“ú \n"
				+ " FROM ‚b‚l‚O‚Q•”–å \n"
				+ " WHERE ‰ïˆõ‚b‚c   = '" + sKcode + "' \n"
				+ "   AND •”–å‚b‚c   = '" + sBcode + "' \n"
				+ "   AND íœ‚e‚f   = '0' \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0).Trim();
			}
			else
			{
				sRet[0] = "o‰×“úƒGƒ‰[";
				sRet[1] = "0";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
	
		}

		/*********************************************************************
		 * d•ª‚b‚cæ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA‚c‚aÚ‘±A”­“XA’…“X
		 * –ß’lFƒXƒe[ƒ^ƒXAd•ª‚b‚c
		 *
		 *********************************************************************/
		private static string GET_SIWAKE_SELECT
			= "SELECT d•ª‚b‚c \n"
			+ " FROM ‚b‚l‚P‚Vd•ª \n"
			;

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(2206):
		*/
		private String[] Get_siwake(string[] sUser, OracleConnection conn2, string sHatuCd, string sTyakuCd)
		{
			//			logWriter(sUser, INF, "d•ª‚b‚cæ“¾ŠJn");

			string[] sRet = new string[2];

			string cmdQuery = GET_SIWAKE_SELECT
				+ " WHERE ”­“XŠ‚b‚c = '" + sHatuCd + "' \n"
				+ " AND ’…“XŠ‚b‚c = '" + sTyakuCd + "' \n"
				+ " AND íœ‚e‚f = '0' \n"
				;

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			if(reader.Read())
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0);
			}
			else
			{
				sRet[0] = "d•ª‚b‚c‚ğŒˆ‚ß‚ç‚ê‚Ü‚¹‚ñ‚Å‚µ‚½";
				sRet[1] = " ";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * ZŠƒ}ƒXƒ^ˆê——æ“¾
		 * ˆø”F—X•Ö”Ô†
		 * –ß’lFƒXƒe[ƒ^ƒXAˆê——i—X•Ö”Ô†A“s“¹•{Œ§–¼j...
		 *
		 * QÆŒ³FZŠŒŸõ.cs
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(3993):
		*/
		[WebMethod]
		public String[] Get_byPostcodeM(string[] sUser, string s—X•Ö”Ô†)
		{
			logWriter(sUser, INF, "ZŠƒ}ƒXƒ^ˆê——æ“¾ŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(CM13.—X•Ö”Ô†) || '|' "
					+ "|| TRIM(CM13.“s“¹•{Œ§–¼) || TRIM(CM13.s‹æ’¬‘º–¼) || TRIM(CM13.‘åš’ÊÌ–¼) || '|' "			//ZŠ
					+ "|| TRIM(CM13.“s“¹•{Œ§‚b‚c) || TRIM(CM13.s‹æ’¬‘º‚b‚c) || TRIM(CM13.‘åš’ÊÌ‚b‚c) || '|' "	//ZŠ‚b‚c
					+ "|| NVL(CM10.“XŠ–¼, ' ') || '|' \n"
					+  " FROM ‚b‚l‚P‚RZŠ‚i CM13 \n" // ‰¤q‰^‘—‘Î‰
					+  " LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
					+    " ON CM13.“XŠ‚b‚c = CM10.“XŠ‚b‚c "
					+    "AND CM10.íœ‚e‚f = '0' \n";
				if(s—X•Ö”Ô†.Length == 7)
				{
					cmdQuery += " WHERE CM13.—X•Ö”Ô† = '" + s—X•Ö”Ô† + "' \n";
				}
				else
				{
					cmdQuery +=  " WHERE CM13.—X•Ö”Ô† LIKE '" + s—X•Ö”Ô† + "%' \n";
				}
				cmdQuery +=    " AND CM13.íœ‚e‚f = '0' \n"
					+  " ORDER BY ‘åš’ÊÌ‚b‚c \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0).Trim());
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ZŠƒ}ƒXƒ^ˆê——æ“¾(s)
		 * ˆø”F“s“¹•{Œ§‚b‚cAs‹æ’¬‘º‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXAˆê——i—X•Ö”Ô†A‘åš’ÊÌ–¼j...
		 *
		 *********************************************************************/
		private static string GET_BYKENSHIM_SELECT
			= "SELECT '|' || TRIM(MAX(CM13.—X•Ö”Ô†)) || '|' "
			+ "|| TRIM(MAX(CM13.‘åš’ÊÌ–¼)) || '|' "
			+ "|| TRIM(MAX(CM13.“s“¹•{Œ§‚b‚c))"
			+ "|| TRIM(MAX(CM13.s‹æ’¬‘º‚b‚c))"
			+ "|| TRIM(MAX(CM13.‘åš’ÊÌ‚b‚c)) || '|' "
			+ "|| MIN(NVL(CM10.“XŠ–¼, ' ')) || '|' \n"
			+  " FROM ‚b‚l‚P‚RZŠ‚i CM13 \n" // ‰¤q‰^‘—‘Î‰
			+  " LEFT JOIN ‚b‚l‚P‚O“XŠ CM10 \n"
			+    " ON CM13.“XŠ‚b‚c = CM10.“XŠ‚b‚c "
			+    "AND CM10.íœ‚e‚f = '0' \n"
			;
		private static string GET_BYKENSHIM_WHERE
			= " AND CM13.íœ‚e‚f = '0' \n"
			+ " GROUP BY CM13.‘åš’ÊÌ‚b‚c \n"
			+ " ORDER BY CM13.‘åš’ÊÌ‚b‚c \n"
			;
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(3884):
		*/
		[WebMethod]
		public String[] Get_byKenShiM(string[] sUser, string s“s“¹•{Œ§‚b‚c, string ss‹æ’¬‘º‚b‚c)
		{
			logWriter(sUser, INF, "ZŠƒ}ƒXƒ^ˆê——æ“¾(s)ŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= GET_BYKENSHIM_SELECT
					+ " WHERE CM13.“s“¹•{Œ§‚b‚c = '" + s“s“¹•{Œ§‚b‚c + "' \n"
					+   " AND CM13.s‹æ’¬‘º‚b‚c = '" + ss‹æ’¬‘º‚b‚c + "' \n"
					+ GET_BYKENSHIM_WHERE
					;

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0).Trim());
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

		/*********************************************************************
		 * ‘åš’ÊÌ–¼ˆê——‚Ìæ“¾
		 * ˆø”F“s“¹•{Œ§‚b‚cAs‹æ’¬‘º‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA‘åš’ÊÌ–¼ˆê——
		 *
		 *********************************************************************/
		private static string GET_BYKENSHI_SELECT
			= "SELECT MAX(—X•Ö”Ô†), ‘åš’ÊÌ–¼, ‘åš’ÊÌƒJƒi–¼, MAX(“s“¹•{Œ§‚b‚c), MAX(s‹æ’¬‘º‚b‚c), ‘åš’ÊÌ‚b‚c, MAX(“XŠ‚b‚c) \n"
			+   "FROM ‚b‚l‚P‚RZŠ‚i \n"; // ‰¤q‰^‘—‘Î‰

		private static string GET_BYKENSHI_ORDER
			=    "AND íœ‚e‚f = '0' \n"
			+  "GROUP BY ‘åš’ÊÌ‚b‚c,‘åš’ÊÌ–¼,‘åš’ÊÌƒJƒi–¼ \n"
			+  "ORDER BY ‘åš’ÊÌƒJƒi–¼, ‘åš’ÊÌ‚b‚c \n"
			;

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2address\Service1.asmx.cs(286):
		*/
		[WebMethod]
		public String[] Get_byKenShi(string[] sUser, string s“s“¹•{Œ§‚b‚c, string ss‹æ’¬‘º‚b‚c)
		{
			logWriter(sUser, INF, "‘åš’ÊÌ–¼ˆê——æ“¾ŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKENSHI_SELECT);
				sbQuery.Append(" WHERE “s“¹•{Œ§‚b‚c = '" + s“s“¹•{Œ§‚b‚c + "' \n");
				sbQuery.Append("   AND s‹æ’¬‘º‚b‚c = '" + ss‹æ’¬‘º‚b‚c + "' \n");
				sbQuery.Append(GET_BYKENSHI_ORDER);
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));
					sbRet.Append("|" + reader.GetString(1).Trim());
					sbRet.Append("|D" + "|");
					sbRet.Append(reader.GetString(2).Trim());		// ‘åš’ÊÌƒJƒi–¼
					sbRet.Append("|" + reader.GetString(3).Trim());	// “s“¹•{Œ§‚b‚c
					sbRet.Append(reader.GetString(4).Trim());		// s‹æ’¬‘º‚b‚c
					sbRet.Append(reader.GetString(5).Trim());		// ‘åš’ÊÌ‚b‚c
					sbRet.Append("|" + reader.GetString(6).Trim());	// “XŠ‚b‚c

					sList.Add(sbRet);
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}

		/*********************************************************************
		 * ZŠˆê——‚Ìæ“¾
		 * ˆø”F—X•Ö”Ô†
		 * –ß’lFƒXƒe[ƒ^ƒXAZŠˆê——
		 *
		 *********************************************************************/
		private static string GET_BYPOSTCODE_SELECT
			= "SELECT —X•Ö”Ô†, “s“¹•{Œ§–¼, s‹æ’¬‘º–¼, ‘åš’ÊÌ–¼, ‘åš’ÊÌƒJƒi–¼, “s“¹•{Œ§‚b‚c, s‹æ’¬‘º‚b‚c, ‘åš’ÊÌ‚b‚c, “XŠ‚b‚c \n"
			+  " FROM ‚b‚l‚P‚RZŠ‚i \n"; // ‰¤q‰^‘—‘Î‰

		private static string GET_BYPOSTCODE_ORDER
			=    "AND íœ‚e‚f = '0' \n"
			+  "ORDER BY —X•Ö”Ô†, ‘åš’ÊÌƒJƒi–¼ \n";

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2address\Service1.asmx.cs(415):
		*/
		[WebMethod]
		public String[] Get_byPostcode(string[] sUser, string s—X•Ö”Ô†)
		{
			logWriter(sUser, INF, "ZŠˆê——æ“¾ŠJn");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYPOSTCODE_SELECT);
				if(s—X•Ö”Ô†.Length == 7)
				{
					sbQuery.Append(" WHERE —X•Ö”Ô† = '" + s—X•Ö”Ô† + "' ");
				}
				else
				{
					sbQuery.Append(" WHERE —X•Ö”Ô† LIKE '" + s—X•Ö”Ô† + "%' ");
				}
				sbQuery.Append(GET_BYPOSTCODE_ORDER);

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));		// —X•Ö”Ô†
					sbRet.Append("|" + reader.GetString(1).Trim());	// “s“¹•{Œ§–¼
					sbRet.Append(reader.GetString(2).Trim());		// s‹æ’¬‘º–¼
					sbRet.Append(reader.GetString(3).Trim());		// ‘åš’ÊÌ–¼
					sbRet.Append("|D" + "|");
					sbRet.Append(reader.GetString(4).Trim());		// ‘åš’ÊÌƒJƒi–¼
					sbRet.Append("|" + reader.GetString(5).Trim());	// “s“¹•{Œ§‚b‚c
					sbRet.Append(reader.GetString(6).Trim());		// s‹æ’¬‘º‚b‚c
					sbRet.Append(reader.GetString(7).Trim());		// ‘åš’ÊÌ‚b‚c
					sbRet.Append("|" + reader.GetString(8).Trim());	// “XŠ‚b‚c
					sList.Add(sbRet);

				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				else
				{
					sRet[0] = "³íI—¹";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}
		/*********************************************************************
		 * \î•ñ’Ç‰Á
		 * ˆø”FŠÇ—”Ô†A‰ïˆõ–¼...
		 * –ß’lFƒXƒe[ƒ^ƒXAXV“úAŠÇ—”Ô†
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(5972):
		*/
		[WebMethod]
		public string[] Ins_Mosikomi(string[] sUser, string[] sData)
		{
			//ŠÇ—”Ô†‚Ìæ“¾
			string[] sKey   = {" ", sData[42]};	//“XŠ‚b‚cAXVÒ
			string[] sKanri = Get_KaniSaiban(sUser, sKey);
			if(sKanri[0].Length > 4)
			{
				return sKanri;
			}
			sData[0] = sKanri[1];

			logWriter(sUser, INF, "\î•ñ’Ç‰ÁŠJn");

			OracleConnection conn2 = null;

			string sXV“ú = System.DateTime.Now.ToString("yyyyMMddHHmmss");
			string[] sRet = new string[3]{"", sXV“ú, sData[0]};

			string sXV‚o‚f = "\“o˜^";
			if(sData.Length > 43)
				sXV‚o‚f = sData[43];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT íœ‚e‚f \n"
					+   "FROM ‚r‚l‚O‚T‰ïˆõ\ \n"
					+  "WHERE ŠÇ—”Ô† = " + sData[0] + " \n"
					+    "FOR UPDATE \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				string síœ‚e‚f = "";
				if(reader.Read())
				{
					síœ‚e‚f = reader.GetString(0);
					iCnt++;
				}

				if(iCnt == 1)
				{
					//’Ç‰Á
					cmdQuery
						= "INSERT INTO ‚r‚l‚O‚T‰ïˆõ\ \n"
						+ " VALUES ( " + sData[0] + "  " 
						+         ",'" + sData[1] + "' "
						+         ",'" + sData[2] + "' "
						+         ",'" + sData[3] + "' "
						+         ",'" + sData[4] + "' \n"
						+         ",'" + sData[5] + "' "
						+         ",'" + sData[6] + "' "
						+         ",'" + sData[7] + "' "
						+         ",'" + sData[8] + "' "
						+         ",'" + sData[9] + "' \n"
						+         ",'" + sData[10] + "' "
						+         ",'" + sData[11] + "' "
						+         ",'" + sData[12] + "' "
						+         ",'" + sData[13] + "' "
						+         ",'" + sData[14] + "' \n"
						+         ",'" + sData[15] + "' "
						+         ",'" + sData[16] + "' "
						+         ",'" + sData[17] + "' "
						+         ",'" + sData[18] + "' "
						+         ",'" + sData[19] + "' \n"
						+         ",'" + sData[20] + "' "
						+         ",'" + sData[21] + "' "
						+         ",'" + sData[22] + "' "
						+         ",'" + sData[23] + "' "
						+         ",'" + sData[24] + "' \n"
						+         ",'" + sData[25] + "' "
						+         ",'" + sData[26] + "' "
						+         ",'" + sData[27] + "' "
						+         ",'" + sData[28] + "' "
						+         ",'" + sData[29] + "' \n"
						+         ", " + sData[30] + "  "
						+         ",'" + sData[31] + "' "
						+         ",'" + sData[32] + "' "
						+         ",'" + sData[33] + "' "
						+         ",'" + sData[34] + "' \n"
						+         ", " + sData[35] + "  "
						+         ",'" + sData[36] + "' "
						+         ",'" + sData[37] + "' "
						+         ",'" + sData[38] + "' "
						+         ",'" + sData[39] + "' \n"
						+         ",'" + sData[40] + "' \n"
						+         ",'0' \n"
						+         "," + sXV“ú
						+         ",'" + sXV‚o‚f + "' "
						+         ",'" + sData[42] + "' \n"
						+         "," + sXV“ú
						+         ",'" + sXV‚o‚f + "' "
						+         ",'" + sData[42] + "' \n"
						+ " ) \n";

					CmdUpdate(sUser, conn2, cmdQuery);

					tran.Commit();
					sRet[0] = "³íI—¹";


				}
				else
				{
					//’Ç‰ÁXV
					if (síœ‚e‚f.Equals("1"))
					{
						cmdQuery
							= "UPDATE ‚r‚l‚O‚T‰ïˆõ\ \n"
							+   " SET “XŠ‚b‚c = '" + sData[1] + "' \n"
							+       ",\ÒƒJƒi = '" + sData[2] + "' \n"
							+       ",\Ò–¼ = '" + sData[3] + "' \n"
							+       ",\Ò—X•Ö”Ô† = '" + sData[4] + "' \n"
							+       ",\ÒŒ§‚b‚c = '" + sData[5] + "' \n"
							+       ",\ÒZŠ‚P = '" + sData[6] + "' \n"
							+       ",\ÒZŠ‚Q = '" + sData[7] + "' \n"
							+       ",\Ò“d˜b‚P = '" + sData[8] + "' \n"
							+       ",\Ò“d˜b‚Q = '" + sData[9] + "' \n"
							+       ",\Ò“d˜b‚R = '" + sData[10] + "' \n"
							+       ",\Ò“d˜b = '" + sData[11] + "' \n"
							+       ",\Ò‚e‚`‚w‚P = '" + sData[12] + "' \n"
							+       ",\Ò‚e‚`‚w‚Q = '" + sData[13] + "' \n"
							+       ",\Ò‚e‚`‚w‚R = '" + sData[14] + "' \n"
							+       ",İ’uêŠ‹æ•ª = '" + sData[15] + "' \n"
							+       ",İ’uêŠƒJƒi = '" + sData[16] + "' \n"
							+       ",İ’uêŠ–¼ = '" + sData[17] + "' \n"
							+       ",İ’uêŠ—X•Ö”Ô† = '" + sData[18] + "' \n"
							+       ",İ’uêŠŒ§‚b‚c = '" + sData[19] + "' \n"
							+       ",İ’uêŠZŠ‚P = '" + sData[20] + "' \n"
							+       ",İ’uêŠZŠ‚Q = '" + sData[21] + "' \n"
							+       ",İ’uêŠ“d˜b‚P = '" + sData[22] + "' \n"
							+       ",İ’uêŠ“d˜b‚Q = '" + sData[23] + "' \n"
							+       ",İ’uêŠ“d˜b‚R = '" + sData[24] + "' \n"
							+       ",İ’uêŠ‚e‚`‚w‚P = '" + sData[25] + "' \n"
							+       ",İ’uêŠ‚e‚`‚w‚Q = '" + sData[26] + "' \n"
							+       ",İ’uêŠ‚e‚`‚w‚R = '" + sData[27] + "' \n"
							+       ",İ’uêŠ’S“–Ò–¼ = '" + sData[28] + "' \n"
							+       ",İ’uêŠ–ğE–¼ = '" + sData[29] + "' \n"
							+       ",İ’uêŠg—p—¿ =  " + sData[30] + "  \n"
							+       ",‰ïˆõ‚b‚c = '" + sData[31] + "' \n"
							+       ",g—pŠJn“ú = '" + sData[32] + "' \n"
							+       ",•”–å‚b‚c = '" + sData[33] + "' \n"
							+       ",•”–å–¼ = '" + sData[34] + "' \n"
							+       ",ƒT[ƒ}ƒ‹‘ä” =  " + sData[35] + "  \n"
							+       ",—˜—pÒ‚b‚c = '" + sData[36] + "' \n"
							+       ",—˜—pÒ–¼ = '" + sData[37] + "' \n"
							+       ",ƒpƒXƒ[ƒh = '" + sData[38] + "' \n"
							+       ",³”Fó‘Ô‚e‚f = '" + sData[39] + "' \n"
							+       ",ƒƒ‚ = '" + sData[40] + "' \n"
							+       ",íœ‚e‚f = '0' \n"
							+       ",“o˜^“ú = " + sXV“ú + " \n"
							+       ",“o˜^‚o‚f = '" + sXV‚o‚f + "' \n"
							+       ",“o˜^Ò = '" + sData[42] + "' \n"
							+       ",XV“ú = " + sXV“ú + " \n"
							+       ",XV‚o‚f = '" + sXV‚o‚f + "' \n"
							+       ",XVÒ = '" + sData[42] + "' \n"
							+ " WHERE ŠÇ—”Ô† = '" + sData[0] + "' \n";

						CmdUpdate(sUser, conn2, cmdQuery);

						string sRet‰ïˆõ   = "";
						string sRet•”–å   = "";
						string sRet—˜—pÒ = "";
						//³”Fó‘Ô‚e‚f‚ª[3F³”FÏ]‚Ìê‡
						if(sData[39].Equals("3"))
						{
							sRet‰ïˆõ = Ins_Member2(sUser, conn2, sData, sXV“ú);
							if(sRet‰ïˆõ.Length == 4)
							{
								//•”–åƒ}ƒXƒ^’Ç‰Á
								sRet•”–å = Ins_Section2(sUser, conn2, sData, sXV“ú);
								if(sRet•”–å.Length == 4)
								{
									//—˜—pÒƒ}ƒXƒ^’Ç‰Á
									sRet—˜—pÒ = Ins_User2(sUser, conn2, sData, sXV“ú);
								}
							}
						}
						if(sRet‰ïˆõ.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "‚¨‹q—lF" + sRet‰ïˆõ;
						}
						else if(sRet•”–å.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "ƒZƒNƒVƒ‡ƒ“F" + sRet•”–å;
						}
						else if(sRet—˜—pÒ.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "ƒ†[ƒU[F" + sRet—˜—pÒ;
						}
						else
						{
							tran.Commit();
							sRet[0] = "³íI—¹";

						}
					}
					else
					{
						tran.Rollback();
						sRet[0] = "Šù‚É“o˜^‚³‚ê‚Ä‚¢‚Ü‚·";
					}
				}
				disposeReader(reader);
				reader = null;
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
		/*********************************************************************
		 * ‰ïˆõƒ}ƒXƒ^’Ç‰Á‚Q
		 * ˆø”F‰ïˆõ‚b‚cA‰ïˆõ–¼...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(6792):
		*/
		private string Ins_Member2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			//‰ïˆõƒ}ƒXƒ^’Ç‰Á
			string[] sKey = new string[4]{
											 sData[31],	//‰ïˆõ‚b‚c
											 sData[3],	//\Ò–¼
											 sData[32],	//g—pŠJn“ú
											 sData[42]	//“o˜^ÒAXVÒ
										 };

			string sRet = "";

			string cmdQuery = "";
			cmdQuery
				= "SELECT íœ‚e‚f \n"
				+   "FROM ‚b‚l‚O‚P‰ïˆõ \n"
				+  "WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string síœ‚e‚f = "";
			while (reader.Read())
			{
				síœ‚e‚f = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//’Ç‰Á
				cmdQuery
					= "INSERT INTO ‚b‚l‚O‚P‰ïˆõ \n"
					+ " VALUES ('" + sKey[0] + "' "		//‰ïˆõ‚b‚c
					+         ",'" + sKey[1] + "' "		//‰ïˆõ–¼
					+         ",'" + sKey[2] + "' "		//g—pŠJn“ú
					+         ",'99999999' "			//g—pI—¹“ú
					+         ",'3' \n"					//ŠÇ—Ò‹æ•ª // 3:‰¤qˆê”Ê
					+         ",'0' "
					+         ",'0' "
					+         ",'0' "
					+         ",'0' "
					+         ",'0' \n"
					+         ",'0' "
					+         ",'0' "
					+         ",' ' "
					+         ", 0 "
					+         ", 0 \n"
					+         ", 0 "
					+         ", 0 "
					+         ", 0 \n"
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[3] + "' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[3] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);

				sRet = "³íI—¹";
			}
			else
			{
				//’Ç‰ÁXV
				if (síœ‚e‚f.Equals("1"))
				{
					cmdQuery
						= "UPDATE ‚b‚l‚O‚P‰ïˆõ \n"
						+   " SET ‰ïˆõ–¼ = '" + sKey[1] + "' \n"
						+       ",g—pŠJn“ú = '" + sKey[2] + "' \n"
						+       ",g—pI—¹“ú = '99999999' \n"
						+       ",ŠÇ—Ò‹æ•ª = '3' \n" // 3:‰¤qˆê”Ê
						// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
						+       ",‹L–˜AŒg‚e‚f = '0' \n"
						+       ",•Û—¯ˆóü‚e‚f = '0' \n"
						// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
						+       ",íœ‚e‚f = '0' \n"
						+       ",“o˜^“ú = " + sUpdateTime
						+       ",“o˜^‚o‚f = '‰ïˆõ“o˜^' "
						+       ",“o˜^Ò = '" + sKey[3] + "' \n"
						+       ",XV“ú = " + sUpdateTime
						+       ",XV‚o‚f = '‰ïˆõ“o˜^' "
						+       ",XVÒ = '" + sKey[3] + "' \n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);

					sRet = "³íI—¹";
				}
				else
				{
					sRet = "Šù‚É“o˜^‚³‚ê‚Ä‚¢‚Ü‚·";
				}
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * ŠÇ—”Ô†‚ÌÌ”Ô
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(6655):
		*/
		[WebMethod]
		public String[] Get_KaniSaiban(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "ŠÇ—”Ô†‚Ìæ“¾ŠJn");
			
			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				decimal iƒJƒŒƒ“ƒg”Ô† = 0;
				decimal iŠJn”Ô†     = 0;
				decimal iI—¹”Ô†     = 0;

				string cmdQuery
					= "SELECT ƒJƒŒƒ“ƒg”Ô†, ŠJn”Ô†, I—¹”Ô† \n"
					+ " FROM ‚b‚l‚P‚U“XŠÌ”ÔŠÇ— \n"
					+ " WHERE Ì”Ô‹æ•ª = '01' \n"
					+ " AND “XŠ‚b‚c = '" + sKey[0] + "' \n"
					+ " FOR UPDATE \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				string updQuery = "";
				if(reader.Read())
				{
					iƒJƒŒƒ“ƒg”Ô† = reader.GetDecimal(0);
					iŠJn”Ô†     = reader.GetDecimal(1);
					iI—¹”Ô†     = reader.GetDecimal(2);

					if(iƒJƒŒƒ“ƒg”Ô† < iI—¹”Ô†)
					{
						iƒJƒŒƒ“ƒg”Ô†++;
					}
					else
					{
						iƒJƒŒƒ“ƒg”Ô† = iŠJn”Ô†;
					}
					sRet[1] = iƒJƒŒƒ“ƒg”Ô†.ToString("0000000");

					updQuery 
						= "UPDATE ‚b‚l‚P‚U“XŠÌ”ÔŠÇ— SET \n"
						+ "  ƒJƒŒƒ“ƒg”Ô† = " + iƒJƒŒƒ“ƒg”Ô† + " \n"
						+ ", ŠJn”Ô† = " + iŠJn”Ô† + " \n"
						+ ", I—¹”Ô† = " + iI—¹”Ô† + " \n"
						+ ", XV“ú = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ ", XV‚o‚f = '‰ïˆõ\' \n"
						+ ", XVÒ = '" + sKey[1] + "' \n"
						+ " WHERE Ì”Ô‹æ•ª = '01' \n"
						+ " AND “XŠ‚b‚c = '" + sKey[0] + "' \n"
						+ " AND íœ‚e‚f = '0' \n";
				}
				else
				{
					iƒJƒŒƒ“ƒg”Ô† = 5005001;
					iŠJn”Ô†     = 1000001;
					iI—¹”Ô†     = 9999999;
					sRet[1] = iƒJƒŒƒ“ƒg”Ô†.ToString("0000000");

					// ‘—‚èóÌ”Ô‚Ì’Ç‰Á
					updQuery 
						= "INSERT INTO ‚b‚l‚P‚U“XŠÌ”ÔŠÇ— VALUES( \n"
						+ " '01' \n"
						+ ",'" + sKey[0] + "' \n"
						+ ", " + iƒJƒŒƒ“ƒg”Ô† + " \n"
						+ ", " + iŠJn”Ô† + " \n"
						+ ", " + iI—¹”Ô† + " \n"
						+ ",'0' \n"
						+ ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
						+ ",'‰ïˆõ\' "
						+ ",'" + sKey[1] + "' \n"
						+ ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
						+ ",'‰ïˆõ\' "
						+ ",'" + sKey[1] + "' \n"
						+ ") \n";
				}
				CmdUpdate(sUser, conn2, updQuery);
				disposeReader(reader);
				reader = null;
				tran.Commit();
				sRet[0] = "³íI—¹";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * •”–åƒ}ƒXƒ^’Ç‰Á‚Q
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(6904):
		*/
		private string Ins_Section2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			string[] sKey = new string[10]{
											  sData[31],	//‰ïˆõ‚b‚c
											  sData[33],	//•”–å‚b‚c
											  sData[34],	//•”–å–¼
											  sData[18],	//İ’uêŠ—X•Ö”Ô†
											  sData[20],	//İ’uêŠZŠ‚P
											  sData[21],	//İ’uêŠZŠ‚Q
											  sData[35],	//ƒT[ƒ}ƒ‹‘ä”
											  sData[42]	//“o˜^ÒAXVÒ
											  ,sData[30]	//İ’uêŠg—p—¿
											  ,sData[0]	//ŠÇ—”Ô†
										  };
			string sRet = "";

			string cmdQuery = "";

			cmdQuery
				= "SELECT íœ‚e‚f \n"
				+   "FROM ‚b‚l‚O‚Q•”–å \n"
				+  "WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
				+    "AND •”–å‚b‚c = '" + sKey[1] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string síœ‚e‚f = "";
			while (reader.Read())
			{
				síœ‚e‚f = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//’Ç‰Á
				cmdQuery
					= "INSERT INTO ‚b‚l‚O‚Q•”–å \n"
					+         "(‰ïˆõ‚b‚c \n"
					+         ",•”–å‚b‚c \n"
					+         ",•”–å–¼ \n"
					+         ",‘gD‚b‚c \n"
					+         ",o—Í‡ \n"
					+         ",—X•Ö”Ô† \n"
					+         ",\"ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú\" \n"
					+         ",\"ƒWƒƒ[ƒiƒ‹‚m‚nŠÇ—\" \n"
					+         ",—Œ^‚m‚n \n"
					+         ",o‰×“ú \n"
					+         ",İ’uæZŠ‚P \n"
					+         ",İ’uæZŠ‚Q \n"
					+         ",ƒT[ƒ}ƒ‹‘ä” \n"
					+         ",íœ‚e‚f \n"
					+         ",“o˜^“ú \n"
					+         ",“o˜^‚o‚f \n"
					+         ",“o˜^Ò \n"
					+         ",XV“ú \n"
					+         ",XV‚o‚f \n"
					+         ",XVÒ \n"
					+         ") \n"
					+ " VALUES ('" + sKey[0] + "' "				//‰ïˆõ‚b‚c
					+         ",'" + sKey[1] + "' "				//•”–å‚b‚c
					+         ",'" + sKey[2] + "' "				//•”–å–¼
					+         ",' ' "							//‘gD‚b‚c
					+         ", 0 \n"							//o—Í‡
					+         ",'" + sKey[3] + "' "				//—X•Ö”Ô†
					+         ",TO_CHAR(SYSDATE,'YYYYMMDD') "	//ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú
					+         ", 0 "							//ƒWƒƒ[ƒiƒ‹ŠÇ—‚m‚n
					+         ", 0 "							//—Œ^‚m‚n
					+         ",TO_CHAR(SYSDATE,'YYYYMMDD') \n"	//o‰×“ú
					+         ",'" + sKey[4] + "' "				//İ’uæZŠ‚P
					+         ",'" + sKey[5] + "' "				//İ’uæZŠ‚Q
					+         ", " + sKey[6] + " \n"			//ƒT[ƒ}ƒ‹‘ä”
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[7] + "' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[7] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);

				cmdQuery
					= "INSERT INTO ‚b‚l‚O‚U•”–åŠg’£ \n"
					+         "(‰ïˆõ‚b‚c \n"
					+         ",•”–å‚b‚c \n"
					+         ",g—p—¿ \n"
					+         ",‰ïˆõ\ŠÇ—”Ô† \n"
					+         ",íœ‚e‚f \n"
					+         ",“o˜^“ú \n"
					+         ",“o˜^‚o‚f \n"
					+         ",“o˜^Ò \n"
					+         ",XV“ú \n"
					+         ",XV‚o‚f \n"
					+         ",XVÒ \n"
					+         ") \n"
					+ " VALUES ('" + sKey[0] + "' "				//‰ïˆõ‚b‚c
					+         ",'" + sKey[1] + "' "				//•”–å‚b‚c
					+         ", " + sKey[8] + " \n"			//g—p—¿
					+         ", " + sKey[9] + " \n"			//‰ïˆõ\ŠÇ—”Ô†
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[7] + "' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[7] + "' \n"
					+ " ) \n";
				CmdUpdate(sUser, conn2, cmdQuery);

				sRet = "³íI—¹";
			}
			else
			{
				//’Ç‰ÁXV
				if (síœ‚e‚f.Equals("1"))
				{
					cmdQuery
						= "UPDATE ‚b‚l‚O‚Q•”–å \n"
						+   " SET •”–å–¼ = '" + sKey[2] + "' \n"
						+       ",‘gD‚b‚c = ' ' \n"
						+       ",o—Í‡ = 0 \n"
						+       ",—X•Ö”Ô† = '" + sKey[3] + "' \n"
						+       ",ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+       ",ƒWƒƒ[ƒiƒ‹‚m‚nŠÇ— = 0 \n"
						+       ",—Œ^‚m‚n = 0 \n"
						+       ",o‰×“ú = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+       ",İ’uæZŠ‚P = '" + sKey[4] + "' \n"
						+       ",İ’uæZŠ‚Q = '" + sKey[5] + "' \n"
						+       ",ƒT[ƒ}ƒ‹‘ä” =  " + sKey[6] + " \n"
						+       ",íœ‚e‚f = '0' \n"
						+       ",“o˜^“ú = " + sUpdateTime
						+       ",“o˜^‚o‚f = '‰ïˆõ“o˜^' "
						+       ",“o˜^Ò = '" + sKey[7] + "' \n"
						+       ",XV“ú = " + sUpdateTime
						+       ",XV‚o‚f = '‰ïˆõ“o˜^' "
						+       ",XVÒ = '" + sKey[7] + "'\n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
						+   " AND •”–å‚b‚c = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					cmdQuery
						= "UPDATE ‚b‚l‚O‚U•”–åŠg’£ SET \n"
						+       " g—p—¿ = " + sKey[8] + " \n"
						+       ",‰ïˆõ\ŠÇ—”Ô† = " + sKey[9] + " \n"
						+       ",íœ‚e‚f = '0' \n"
						+       ",“o˜^“ú = " + sUpdateTime
						+       ",“o˜^‚o‚f = '‰ïˆõ“o˜^' "
						+       ",“o˜^Ò = '" + sKey[7] + "' \n"
						+       ",XV“ú = " + sUpdateTime
						+       ",XV‚o‚f = '‰ïˆõ“o˜^' "
						+       ",XVÒ = '" + sKey[7] + "'\n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
						+   " AND •”–å‚b‚c = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "³íI—¹";
				}
				else
				{
					sRet = "Šù‚É“o˜^‚³‚ê‚Ä‚¢‚Ü‚·";
				}
			}
			disposeReader(reader);
			reader = null;

			//ƒGƒ‰[‚ÍAI—¹
			if (!sRet.Equals("³íI—¹")) return sRet;

			logWriter(sUser, INF, "‹L–‚Ì‰Šúƒf[ƒ^“o˜^ŠJn");

			//‹L–‚Ì‰Šúƒf[ƒ^‚ÌŒŸõ
			cmdQuery
				= "SELECT ‹L–‚b‚c \n"
				+      ", ‹L– \n"
				+   "FROM ‚r‚l‚O‚R‹L– \n"
				+  "WHERE ‰ïˆõ‚b‚c = 'default' \n"
				+    "AND •”–å‚b‚c = '0000' \n"
				+    "AND íœ‚e‚f = '0' \n";

			OracleDataReader readerDef = CmdSelect(sUser, conn2, cmdQuery);
			string s‰Šú‹L–‚b‚c = "";
			string s‰Šú‹L–     = "";
			while (readerDef.Read())
			{
				s‰Šú‹L–‚b‚c = readerDef.GetString(0);
				s‰Šú‹L–     = readerDef.GetString(1);

				//‹L–‚ÌŒŸõ
				cmdQuery
					= "SELECT ‹L–‚b‚c \n"
					+   "FROM ‚r‚l‚O‚R‹L– \n"
					+  "WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
					+    "AND •”–å‚b‚c = '" + sKey[1] + "' \n"
					+    "AND ‹L–‚b‚c = '" + s‰Šú‹L–‚b‚c + "' \n"
					+    "FOR UPDATE \n";

				OracleDataReader readerNote = CmdSelect(sUser, conn2, cmdQuery);
				if (readerNote.Read())
				{
					//Šù‚É‹L–‚ª‚ ‚éê‡‚ÍV‹KXV
					cmdQuery
						= "UPDATE ‚r‚l‚O‚R‹L– \n"
						+   " SET ‹L– = '" + s‰Šú‹L– + "' \n"
						+       ",íœ‚e‚f = '0' \n"
						+       ",“o˜^“ú = " + sUpdateTime
						+       ",“o˜^‚o‚f = '‰Šú‹L–' \n"
						+       ",“o˜^Ò = '" + sKey[7] + "' \n"
						+       ",XV“ú = " + sUpdateTime
						+       ",XV‚o‚f = '‰Šú‹L–' \n"
						+       ",XVÒ = '" + sKey[7] + "' \n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
						+   " AND •”–å‚b‚c = '" + sKey[1] + "' \n"
						+   " AND ‹L–‚b‚c = '" + s‰Šú‹L–‚b‚c + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "³íI—¹";
				}
				else
				{
					//V‹K’Ç‰Á
					cmdQuery
						= "INSERT INTO ‚r‚l‚O‚R‹L– \n"
						+ " VALUES ('" + sKey[0] + "' " 
						+         ",'" + sKey[1] + "' "
						+         ",'" + s‰Šú‹L–‚b‚c + "' "
						+         ",'" + s‰Šú‹L– + "' \n"
						+         ",'0' \n"
						+         "," + sUpdateTime
						+         ",'‰Šú‹L–' "
						+         ",'" + sKey[7] + "' \n"
						+         "," + sUpdateTime
						+         ",'‰Šú‹L–' "
						+         ",'" + sKey[7] + "' \n"
						+ " ) \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "³íI—¹";
				}
				disposeReader(readerNote);
				readerNote = null;
			}
			disposeReader(readerDef);
			readerDef = null;

			return sRet;
		}

		/*********************************************************************
		 * —˜—pÒƒ}ƒXƒ^’Ç‰Á‚Q
		 * ˆø”F‰ïˆõ‚b‚cA—˜—pÒ‚b‚cA—˜—pÒ–¼
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(7197):
		*/
		private string Ins_User2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			string[] sKey = new string[6]{
											 sData[31],	//‰ïˆõ‚b‚c
											 sData[36],	//—˜—pÒ‚b‚c
											 sData[38],	//ƒpƒXƒ[ƒh
											 sData[37],	//—˜—pÒ–¼
											 sData[33],	//•”–å‚b‚c
											 sData[42]	//“o˜^ÒAXVÒ
										 };
			string sRet = "";

			string cmdQuery = "";

			cmdQuery
				= "SELECT íœ‚e‚f \n"
				+   "FROM ‚b‚l‚O‚S—˜—pÒ \n"
				+  "WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
				+    "AND —˜—pÒ‚b‚c = '" + sKey[1] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string síœ‚e‚f = "";
			while (reader.Read())
			{
				síœ‚e‚f = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//’Ç‰Á
				cmdQuery
					= "INSERT INTO ‚b‚l‚O‚S—˜—pÒ \n"
					+ " VALUES ('" + sKey[0] + "' "		//‰ïˆõ‚b‚c
					+         ",'" + sKey[1] + "' "		//—˜—pÒ‚b‚c
					+         ",'" + sKey[2] + "' "		//ƒpƒXƒ[ƒh
					+         ",'" + sKey[3] + "' "		//—˜—pÒ–¼
					+         ",'" + sKey[4] + "' \n"	//•”–å‚b‚c
					+         ",' ' "					//‰×‘—l‚b‚c
					+         ",0 "						//”FØƒGƒ‰[‰ñ”
					+         ",' ' "					//Œ ŒÀ‚P
					+         ",' ' "
					+         ",' ' \n"
					+         ",' ' "
					+         ",' ' "
					+         ",' ' "
					+         ",' ' "
					+         ",' ' \n"
					+         ",' ' "
					+         ",' ' \n"
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'"+ sUpdateTime.Substring(0,8) +"' "
					+         ",'" + sKey[5] + "' \n"
					+         "," + sUpdateTime
					+         ",'‰ïˆõ“o˜^' "
					+         ",'" + sKey[5] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);
				sRet = "³íI—¹";
			}
			else
			{
				//’Ç‰ÁXV
				if (síœ‚e‚f.Equals("1"))
				{
					cmdQuery
						= "UPDATE ‚b‚l‚O‚S—˜—pÒ \n"
						+   " SET ƒpƒXƒ[ƒh = '" + sKey[2] + "' \n"
						+       ",—˜—pÒ–¼ = '" + sKey[3] + "' \n"
						+       ",•”–å‚b‚c = '" + sKey[4] + "' \n"
						+       ",‰×‘—l‚b‚c = ' ' \n"
						+       ",”FØƒGƒ‰[‰ñ” = 0 \n"
						+       ",Œ ŒÀ‚P = ' ' \n"
						+       ",íœ‚e‚f = '0' \n"
						+       ",“o˜^“ú = " + sUpdateTime
						+       ",“o˜^‚o‚f = '"+ sUpdateTime.Substring(0,8) +"' "
						+       ",“o˜^Ò = '" + sKey[5] + "' \n"
						+       ",XV“ú = " + sUpdateTime
						+       ",XV‚o‚f = '‰ïˆõ“o˜^' "
						+       ",XVÒ = '" + sKey[5] + "' \n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sKey[0] + "' \n"
						+   " AND —˜—pÒ‚b‚c = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "³íI—¹";
				}
				else
				{
					sRet = "Šù‚É“o˜^‚³‚ê‚Ä‚¢‚Ü‚·";
				}
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * \î•ñXV
		 * ˆø”FŠÇ—”Ô†A‰ïˆõ–¼...
		 * –ß’lFƒXƒe[ƒ^ƒXAXV“ú
		 *
		 *********************************************************************/
		private static string UPD_MOSIKOMI_SELECT
			= "SELECT ŠÇ—”Ô† "
			+ ", “XŠ‚b‚c "
			+ ", \ÒƒJƒi "
			+ ", \Ò–¼ "
			+ ", \Ò—X•Ö”Ô† \n"
			+ ", \ÒŒ§‚b‚c "
			+ ", \ÒZŠ‚P "
			+ ", \ÒZŠ‚Q "
			+ ", \Ò“d˜b‚P "
			+ ", \Ò“d˜b‚Q \n"
			+ ", \Ò“d˜b‚R "
			+ ", \Ò“d˜b "
			+ ", \Ò‚e‚`‚w‚P "
			+ ", \Ò‚e‚`‚w‚Q "
			+ ", \Ò‚e‚`‚w‚R \n"
			+ ", İ’uêŠ‹æ•ª "
			+ ", İ’uêŠƒJƒi "
			+ ", İ’uêŠ–¼ "
			+ ", İ’uêŠ—X•Ö”Ô† "
			+ ", İ’uêŠŒ§‚b‚c \n"
			+ ", İ’uêŠZŠ‚P "
			+ ", İ’uêŠZŠ‚Q "
			+ ", İ’uêŠ“d˜b‚P "
			+ ", İ’uêŠ“d˜b‚Q "
			+ ", İ’uêŠ“d˜b‚R \n"
			+ ", İ’uêŠ‚e‚`‚w‚P "
			+ ", İ’uêŠ‚e‚`‚w‚Q "
			+ ", İ’uêŠ‚e‚`‚w‚R "
			+ ", İ’uêŠ’S“–Ò–¼ "
			+ ", İ’uêŠ–ğE–¼ \n"
			+ ", İ’uêŠg—p—¿ "
			+ ", ‰ïˆõ‚b‚c "
			+ ", g—pŠJn“ú "
			+ ", •”–å‚b‚c "
			+ ", •”–å–¼ \n"
			+ ", \"ƒT[ƒ}ƒ‹‘ä”\" "
			+ ", —˜—pÒ‚b‚c "
			+ ", —˜—pÒ–¼ "
			+ ", \"ƒpƒXƒ[ƒh\" "
			+ ", ³”Fó‘Ô‚e‚f \n"
			+ ", ƒƒ‚ "
			+ ", TO_CHAR(XV“ú) "
			+ ", XVÒ \n"
			+ "FROM ‚r‚l‚O‚T‰ïˆõ\ \n"
			+ "";

		private static string UPD_MOSIKOMI_DELETE
			= "UPDATE ‚r‚l‚O‚T‰ïˆõ\ \n"
			+ "SET íœ‚e‚f = '1' \n"
			+ "";

		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2maintenance\Service1.asmx.cs(6303):
		*/
		[WebMethod]
		public string[] Upd_Mosikomi(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "\î•ñXVŠJn");

			OracleConnection conn2 = null;
			string sXV“ú = System.DateTime.Now.ToString("yyyyMMddHHmmss");
			string[] sRet = new string[3]{"", sXV“ú, sData[0]};

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			string cmdQuery = "";

			try
			{
				bool bUpdState = false;

				//³”Fó‘Ô‚e‚f‚ª[1F\¿’†]‚Ìê‡iˆóüƒ{ƒ^ƒ“‚Ìj
				if(sData[39].Equals("1"))
				{
					string[] sRefData = new string[43];
					cmdQuery = UPD_MOSIKOMI_SELECT
						+ " WHERE ŠÇ—”Ô† = '" + sData[0] + "' \n"
						+ " AND íœ‚e‚f = '0' \n"
						+ " AND XV“ú = " + sData[41] + " \n"
						+ " FOR UPDATE \n";

					OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read())
					{
						tran.Rollback();
						sRet[0] = "‘¼‚Ì’[––‚ÅXV‚³‚ê‚Ä‚¢‚Ü‚·";
						logWriter(sUser, INF, sRet[0]);
						return sRet;
					}
					sRefData[0] = "";
					//ŠÇ—”Ô†‚Íƒ_ƒ~[
					sRefData[1] = reader.GetString(1).Trim();
					sRefData[2] = reader.GetString(2).Trim();
					sRefData[3] = reader.GetString(3).Trim();
					sRefData[4] = reader.GetString(4).Trim();
					sRefData[5] = reader.GetString(5).Trim();	//\ÒŒ§‚b‚c
					sRefData[6] = reader.GetString(6).Trim();
					sRefData[7] = reader.GetString(7).Trim();
					sRefData[8] = reader.GetString(8).Trim();
					sRefData[9] = reader.GetString(9).Trim();
					sRefData[10] = reader.GetString(10).Trim();	//\Ò“d˜b‚R
					sRefData[11] = reader.GetString(11).Trim();
					sRefData[12] = reader.GetString(12).Trim();
					sRefData[13] = reader.GetString(13).Trim();
					sRefData[14] = reader.GetString(14).Trim();
					sRefData[15] = reader.GetString(15).Trim();	//İ’uêŠ‹æ•ª
					sRefData[16] = reader.GetString(16).Trim();
					sRefData[17] = reader.GetString(17).Trim();
					sRefData[18] = reader.GetString(18).Trim();
					sRefData[19] = reader.GetString(19).Trim();
					sRefData[20] = reader.GetString(20).Trim();	//İ’uêŠZŠ‚P
					sRefData[21] = reader.GetString(21).Trim();
					sRefData[22] = reader.GetString(22).Trim();
					sRefData[23] = reader.GetString(23).Trim();
					sRefData[24] = reader.GetString(24).Trim();
					sRefData[25] = reader.GetString(25).Trim();	//İ’uêŠ‚e‚`‚w‚P
					sRefData[26] = reader.GetString(26).Trim();
					sRefData[27] = reader.GetString(27).Trim();
					sRefData[28] = reader.GetString(28).Trim();
					sRefData[29] = reader.GetString(29).Trim();
					sRefData[30] = reader.GetDecimal(30).ToString().Trim();	//İ’uêŠg—p—¿
					sRefData[31] = reader.GetString(31).Trim();
					sRefData[32] = reader.GetString(32).Trim();
					sRefData[33] = reader.GetString(33).Trim();
					sRefData[34] = reader.GetString(34).Trim();
					sRefData[35] = reader.GetDecimal(35).ToString().Trim();	//ƒT[ƒ}ƒ‹‘ä”
					sRefData[36] = reader.GetString(36).Trim();
					sRefData[37] = reader.GetString(37).Trim();
					sRefData[38] = reader.GetString(38).Trim();
					sRefData[39] = reader.GetString(39).Trim();
					sRefData[40] = reader.GetString(40).Trim();	//ƒƒ‚
					sRefData[41] = reader.GetString(41).Trim();
					sRefData[42] = reader.GetString(42).Trim();

					//³”Fó‘Ô‚e‚fi_:“o˜^’†A1:\¿’†A2:—¯•Û’†A3:³”FÏj‚ª
					//i1:\¿’†‚à‚µ‚­‚Í2:—¯•Û’†‚Ì‚à‚Ìj
					if(sRefData[39].Length > 0)
					{
						//ƒf[ƒ^‚ÌXVó‹µ‚ğƒ`ƒFƒbƒN‚·‚é
						for(int iCnt = 2; iCnt <= 30; iCnt++)
						{
							if(!sRefData[iCnt].Equals(sData[iCnt].Trim()))
							{
								bUpdState = true;
								break;
							}
						}

						if(bUpdState)
						{
							//ƒf[ƒ^íœ
							cmdQuery = UPD_MOSIKOMI_DELETE
								+ ", XV‚o‚f = '\XV' \n"
								+ ", XVÒ   = '" + sData[42] +"' \n"
								+ ", XV“ú = "+ sXV“ú + " \n"
								+ " WHERE ŠÇ—”Ô† = '" + sData[0] + "' \n"
								+ " AND íœ‚e‚f = '0' \n"
								+ " AND XV“ú = " + sData[41] + " \n";

							if (CmdUpdate(sUser, conn2, cmdQuery) == 0)
							{
								tran.Rollback();
								sRet[0] = "‘¼‚Ì’[––‚ÅXV‚³‚ê‚Ä‚¢‚Ü‚·";
							}
							else
							{
								tran.Commit();
								sRet[0] = "³íI—¹";
							}
							logWriter(sUser, INF, sRet[0]);
							//ƒf[ƒ^‚ª•ÏX‚³‚ê‚Ä‚¢‚éê‡‚É‚ÍAV‚µ‚¢ó’‚m‚n‚Åƒf[ƒ^‚ğ’Ç‰Á‚·‚é
							//•Û—¯@ƒgƒ‰ƒ“ƒUƒNƒVƒ‡ƒ“§Œä
							return Ins_Mosikomi(sUser, sData);
						}
					}
					disposeReader(reader);
					reader = null;
				}

				cmdQuery
					= "UPDATE ‚r‚l‚O‚T‰ïˆõ\ \n"
					+   " SET “XŠ‚b‚c = '" + sData[1] + "' \n"
					+       ",\ÒƒJƒi = '" + sData[2] + "' \n"
					+       ",\Ò–¼ = '" + sData[3] + "' \n"
					+       ",\Ò—X•Ö”Ô† = '" + sData[4] + "' \n"
					+       ",\ÒŒ§‚b‚c = '" + sData[5] + "' \n"
					+       ",\ÒZŠ‚P = '" + sData[6] + "' \n"
					+       ",\ÒZŠ‚Q = '" + sData[7] + "' \n"
					+       ",\Ò“d˜b‚P = '" + sData[8] + "' \n"
					+       ",\Ò“d˜b‚Q = '" + sData[9] + "' \n"
					+       ",\Ò“d˜b‚R = '" + sData[10] + "' \n"
					+       ",\Ò“d˜b = '" + sData[11] + "' \n"
					+       ",\Ò‚e‚`‚w‚P = '" + sData[12] + "' \n"
					+       ",\Ò‚e‚`‚w‚Q = '" + sData[13] + "' \n"
					+       ",\Ò‚e‚`‚w‚R = '" + sData[14] + "' \n"
					+       ",İ’uêŠ‹æ•ª = '" + sData[15] + "' \n"
					+       ",İ’uêŠƒJƒi = '" + sData[16] + "' \n"
					+       ",İ’uêŠ–¼ = '" + sData[17] + "' \n"
					+       ",İ’uêŠ—X•Ö”Ô† = '" + sData[18] + "' \n"
					+       ",İ’uêŠŒ§‚b‚c = '" + sData[19] + "' \n"
					+       ",İ’uêŠZŠ‚P = '" + sData[20] + "' \n"
					+       ",İ’uêŠZŠ‚Q = '" + sData[21] + "' \n"
					+       ",İ’uêŠ“d˜b‚P = '" + sData[22] + "' \n"
					+       ",İ’uêŠ“d˜b‚Q = '" + sData[23] + "' \n"
					+       ",İ’uêŠ“d˜b‚R = '" + sData[24] + "' \n"
					+       ",İ’uêŠ‚e‚`‚w‚P = '" + sData[25] + "' \n"
					+       ",İ’uêŠ‚e‚`‚w‚Q = '" + sData[26] + "' \n"
					+       ",İ’uêŠ‚e‚`‚w‚R = '" + sData[27] + "' \n"
					+       ",İ’uêŠ’S“–Ò–¼ = '" + sData[28] + "' \n"
					+       ",İ’uêŠ–ğE–¼ = '" + sData[29] + "' \n"
					+       ",İ’uêŠg—p—¿ =  " + sData[30] + "  \n"
					+       ",‰ïˆõ‚b‚c = '" + sData[31] + "' \n"
					+       ",g—pŠJn“ú = '" + sData[32] + "' \n"
					+       ",•”–å‚b‚c = '" + sData[33] + "' \n"
					+       ",•”–å–¼ = '" + sData[34] + "' \n"
					+       ",ƒT[ƒ}ƒ‹‘ä” =  " + sData[35] + "  \n"
					+       ",—˜—pÒ‚b‚c = '" + sData[36] + "' \n"
					+       ",—˜—pÒ–¼ = '" + sData[37] + "' \n"
					+       ",ƒpƒXƒ[ƒh = '" + sData[38] + "' \n"
					+       ",³”Fó‘Ô‚e‚f = '" + sData[39] + "' \n"
					+       ",ƒƒ‚ = '" + sData[40] + "' \n"
					+       ",XV“ú = " + sXV“ú + " \n"
					+       ",XV‚o‚f = '\XV' \n"
					+       ",XVÒ = '" + sData[42] + "' \n"
					+ " WHERE ŠÇ—”Ô† = '" + sData[0] + "' \n"
					+   " AND íœ‚e‚f = '0' \n"
					+   " AND XV“ú = " + sData[41] + " \n";

				if (CmdUpdate(sUser, conn2, cmdQuery) != 0)
				{
					string sRet‰ïˆõ   = "";
					string sRet•”–å   = "";
					string sRet—˜—pÒ = "";
					//³”Fó‘Ô‚e‚f‚ª[3F³”FÏ]‚Ìê‡
					if(sData[39].Equals("3"))
					{
						sRet‰ïˆõ = Ins_Member2(sUser, conn2, sData, sXV“ú);
						if(sRet‰ïˆõ.Length == 4)
						{
							//•”–åƒ}ƒXƒ^’Ç‰Á
							sRet•”–å = Ins_Section2(sUser, conn2, sData, sXV“ú);
							if(sRet•”–å.Length == 4)
							{
								//—˜—pÒƒ}ƒXƒ^’Ç‰Á
								sRet—˜—pÒ = Ins_User2(sUser, conn2, sData, sXV“ú);
							}
						}
					}
					if(sRet‰ïˆõ.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "‚¨‹q—lF" + sRet‰ïˆõ;
					}
					else if(sRet•”–å.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "ƒZƒNƒVƒ‡ƒ“F" + sRet•”–å;
					}
					else if(sRet—˜—pÒ.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "ƒ†[ƒU[F" + sRet—˜—pÒ;
					}
					else
					{
						tran.Commit();
						sRet[0] = "³íI—¹";
						sRet[1] = sXV“ú;
					}
				}
				else
				{
					tran.Rollback();
					sRet[0] = "‘¼‚Ì’[––‚ÅXV‚³‚ê‚Ä‚¢‚Ü‚·";
				}
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
		/*********************************************************************
		 * ©“®o‰×“o˜^—pZŠæ“¾‚R
		 * @@‚r‚l‚O‚Q‰×ólA‚b‚l‚P‚S—X•Ö”Ô†A‚b‚l‚P‚T’…“X”ñ•\¦A‚b‚l‚P‚X—X•ÖZŠ
		 *     ‚Ì‚Rƒ}ƒXƒ^‚ğg—p‚µ‚Ä’…“XƒR[ƒh‚ğŒˆ’è‚·‚éB
		 * ˆø”F‰ïˆõƒR[ƒhA•”–åƒR[ƒhA‰×ólƒR[ƒhA—X•Ö”Ô†AZŠA–¼
		 * –ß’lFƒXƒe[ƒ^ƒXA“XŠ‚b‚cA“XŠ–¼AZŠ‚b‚c
		 *
		 * Create : 2008.06.12 kcl)X–{
		 * @@@@@@Get_autoEntryPref ‚ğŒ³‚Éì¬
		 * Modify : 2008.12.25 kcl)X–{
		 *            ˆø”‚É–¼‚ğ’Ç‰Á
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2syukka\Service1.asmx.cs(5201):
		*/
		[WebMethod]
		public string [] Get_autoEntryPref3(string [] sUser, 
			string sKaiinCode, string sBumonCode, string sNiukeCode, 
			string sYuubin, string sJuusyo, string sShimei)
		{
			// ƒƒOo—Í
			logWriter(sUser, INF, "ZŠæ“¾‚RŠJn");

			OracleConnection conn2 = null;
			string [] sRet = new string [4];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				// ‚c‚aÚ‘±‚É¸”s
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			try 
			{
				// ’…“XƒR[ƒh‚Ìæ“¾
				string [] sResult = this.Get_tyakuten3(sUser, conn2, sKaiinCode, sBumonCode, sNiukeCode, sYuubin, sJuusyo, sShimei);

				if (sResult[0] == " ")
				{
					// æ“¾¬Œ÷
					sRet[1] = sResult[3];	// ZŠ‚b‚c
					sRet[2] = sResult[1];	// “XŠ‚b‚c
					sRet[3] = sResult[2];	// “XŠ–¼

					sRet[0] = "³íI—¹";
				}
				else
				{
					// æ“¾¸”s
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}

				// ƒƒOo—Í
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				// Oracle ‚ÌƒGƒ‰[
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				// ‚»‚êˆÈŠO‚ÌƒGƒ‰[
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				// I—¹ˆ—
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}
		// MOD 2011.06.06 “Œ“sj‚–Ø ‰¤q‰^‘——A‘—¤•iƒR[ƒhŒŸõ’Ç‰Á START
		/*********************************************************************
		 * —A‘—¤•iƒR[ƒhŒŸõ
		 * ˆø”F•”–å‚b‚cA‹L–
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *       —A‘—¤•i–¼‚©‚ç—A‘—¤•iƒR[ƒh‚ğŒŸõ‚µ‚Ü‚·
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2kiji\Service1.asmx.cs(571):
		*/
		[WebMethod]
		public String[] Get_kijiCD(string[] sUser, string sBcode, string sKname)
		{
			logWriter(sUser, INF, "—A‘—¤•iƒR[ƒhŒŸõŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string sKcode = "";

				if(sBcode.Equals("100"))
				{
					if(sKname.StartsWith("ŠÔw’è"))
					{
						if(sKname.EndsWith("‚Ü‚Å"))
						{
							sKcode = "11X";
						}
						else if(sKname.EndsWith("ˆÈ~"))
						{
							sKcode = "12X";
						}
					}
				}
				else if(sBcode.Equals("200"))
				{
					if(sKname.StartsWith("ŠÔw’è"))
					{
						if(sKname.EndsWith("‚Ü‚Å"))
						{
							sKcode = "21X";
						}
						else if(sKname.EndsWith("ˆÈ~"))
						{
							sKcode = "22X";
						}
					}
				}

				sbQuery.Append( "SELECT ‹L–‚b‚c" );
				sbQuery.Append(  " FROM ‚r‚l‚O‚R‹L– \n" );
				sbQuery.Append( " WHERE ‰ïˆõ‚b‚c = 'Jyusoshohin' \n" ); // ‰¤q‰^‘—‘Î‰
				sbQuery.Append(   " AND •”–å‚b‚c = '" + sBcode +"' \n" );
				if (sKcode.Length != 0)
				{
					sbQuery.Append(   " AND ‹L–‚b‚c = '" + sKcode +"' \n" );
				}
				else
				{
					sbQuery.Append(   " AND ‹L–     = '" + sKname +"' \n" );
				}
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				if(reader.Read())
				{
					sRet[0] = "³íI—¹";
					sRet[1] = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				disposeReader(reader);
				reader = null;

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

		/*********************************************************************
		 * ‹L–ˆóüƒf[ƒ^æ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚c
		 * –ß’lFƒXƒe[ƒ^ƒXA‹L–‚b‚cA‹L–
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2print\Service1.asmx.cs(1520):
		*/
		[WebMethod]
		public ArrayList Get_NotePrintData(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "‹L–ˆóüƒf[ƒ^æ“¾ŠJn");

			OracleConnection conn2 = null;
			ArrayList alRet = new ArrayList();
			string[] sRet = new string[1];
			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				alRet.Add(sRet);
				return alRet;
			}

			try
			{
				//—A‘—w¦‚Ìæ“¾
				System.Text.StringBuilder cmdQuery_y = new System.Text.StringBuilder(256);
				cmdQuery_y.Append("SELECT ");
				cmdQuery_y.Append(" SM03_1.‹L–‚b‚c ");
				cmdQuery_y.Append(",SM03_1.‹L– ");
				cmdQuery_y.Append(",NVL(SM03_2.‹L–‚b‚c, ' ') ");
				cmdQuery_y.Append(",NVL(SM03_2.‹L–, ' ') ");
				cmdQuery_y.Append(" FROM \"‚r‚l‚O‚R‹L–\" SM03_1 ");
				cmdQuery_y.Append(" LEFT JOIN \"‚r‚l‚O‚R‹L–\" SM03_2 ");
				cmdQuery_y.Append(       " ON SM03_1.‰ïˆõ‚b‚c = SM03_2.‰ïˆõ‚b‚c ");
				cmdQuery_y.Append(      " AND SM03_1.‹L–‚b‚c = SM03_2.•”–å‚b‚c ");
				cmdQuery_y.Append(      " AND '0'             = SM03_2.íœ‚e‚f ");
				cmdQuery_y.Append("WHERE SM03_1.‰ïˆõ‚b‚c   = 'Jyusoshohin' "); // ‰¤q‰^‘—‘Î‰
				cmdQuery_y.Append(  "AND SM03_1.•”–å‚b‚c   = '0000' ");
				cmdQuery_y.Append(  "AND SM03_1.íœ‚e‚f   = '0' ");
				cmdQuery_y.Append("ORDER BY SM03_1.‹L–‚b‚c,SM03_2.‹L–‚b‚c \n");
				OracleDataReader reader_y = CmdSelect(sUser, conn2, cmdQuery_y);

				//•i–¼‹L–‚Ìæ“¾
				System.Text.StringBuilder cmdQuery_h = new System.Text.StringBuilder(256);
				cmdQuery_h.Append("SELECT ");
				cmdQuery_h.Append(" ‹L–‚b‚c ");
				cmdQuery_h.Append(",‹L– ");
				cmdQuery_h.Append(" FROM \"‚r‚l‚O‚R‹L–\" ");
				cmdQuery_h.Append("WHERE ‰ïˆõ‚b‚c   = '" + sKey[0] + "' ");
				cmdQuery_h.Append(  "AND •”–å‚b‚c   = '" + sKey[1] + "' ");
				cmdQuery_h.Append(  "AND íœ‚e‚f   = '0' ");
				cmdQuery_h.Append("ORDER BY ‹L–‚b‚c \n");
				OracleDataReader reader_h = CmdSelect(sUser, conn2, cmdQuery_h);

				bool b—A‘—w¦ = true;
				bool b•i–¼‹L– = true;
				string se‹L– = "";
				while (true)
				{
					if (b—A‘—w¦) b—A‘—w¦ = reader_y.Read();
					if (b•i–¼‹L–) b•i–¼‹L– = reader_h.Read();

					string[] sData = new string[4];
					if (b—A‘—w¦)
					{
						sData[0]  = reader_y.GetString(0).TrimEnd();
						sData[1]  = reader_y.GetString(1).TrimEnd();
					}
					else
					{
						sData[0] = "";
						sData[1] = "";
					}
					if (b—A‘—w¦ && !sData[0].Equals(se‹L–))
					{
						if (b•i–¼‹L–)
						{
							sData[2]  = reader_h.GetString(0).TrimEnd();
							sData[3]  = reader_h.GetString(1).TrimEnd();
						}
						else
						{
							sData[2] = "";
							sData[3] = "";
						}
						se‹L– = sData[0];
						alRet.Add(sData);
						if (!reader_y.GetString(2).TrimEnd().Equals(""))
						{
							sData = new string[4];
							if (b•i–¼‹L–) b•i–¼‹L– = reader_h.Read();
							sData[0]  = "  " + reader_y.GetString(2).TrimEnd();
							sData[1]  = "@@@" + reader_y.GetString(3).TrimEnd();
						}
						else
						{
							continue;
						}
					}
					else
					{
						if (b—A‘—w¦)
						{
							sData[0]  = "  " + reader_y.GetString(2).TrimEnd();
							sData[1]  = "@@@" + reader_y.GetString(3).TrimEnd();
						}
					}

					if (b•i–¼‹L–)
					{
						sData[2]  = reader_h.GetString(0).TrimEnd();
						sData[3]  = reader_h.GetString(1).TrimEnd();
					}
					else
					{
						sData[2] = "";
						sData[3] = "";
					}
					if (!b—A‘—w¦ && !b•i–¼‹L–) break;
					alRet.Add(sData);
				}
				disposeReader(reader_y);
				disposeReader(reader_h);
				reader_y = null;
				reader_h = null;
				if (alRet.Count == 0)
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
					alRet.Add(sRet);
				}
				else
				{
					sRet[0] = "³íI—¹";
					alRet.Insert(0, sRet);
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
				alRet.Insert(0, sRet);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				alRet.Insert(0, sRet);
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return alRet;
		}
		// MOD 2011.06.06 “Œ“sj‚–Ø ‰¤q‰^‘——A‘—¤•iƒR[ƒhŒŸõ’Ç‰Á END
		// ADD 2015.05.01 BEVAS) ‘O“c CM14J—X•Ö”Ô†‘¶İƒ`ƒFƒbƒN START

		/*********************************************************************
		 * ZŠ‚Ìæ“¾ ‰¤q‘Î‰”Å
		 * ˆø”F—X•Ö”Ô†
		 * –ß’lFƒXƒe[ƒ^ƒXA—X•Ö”Ô†AZŠAZŠ‚b‚c
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2address\Service1.asmx.cs(535):
		*/ 
		// ADD 2005.05.11 “Œ“sj‚–Ø ORA-03113‘ÎôH START
		private static string GET_BYPOSTCODE2J_SELECT
			= "SELECT —X•Ö”Ô†, “s“¹•{Œ§–¼, s‹æ’¬‘º–¼, ’¬ˆæ–¼, \n"
			+ " “s“¹•{Œ§‚b‚c, s‹æ’¬‘º‚b‚c, ‘åš’ÊÌ‚b‚c \n"
			+ " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i \n";
		// ADD 2005.05.11 “Œ“sj‚–Ø ORA-03113‘ÎôH END
		[WebMethod]
		public String[] Get_byPostcode2(string[] sUser, string s—X•Ö”Ô†)
		{
			// DEL 2007.05.10 “Œ“sj‚–Ø –¢g—pŠÖ”‚ÌƒRƒƒ“ƒg‰»
			//			logFileOpen(sUser);
			logWriter(sUser, INF, "ZŠæ“¾ŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ADD-S 2012.09.06 COA)‰¡R OracleƒT[ƒo•‰‰×ŒyŒ¸‘ÎôiSQL‚ÉƒoƒCƒ“ƒh•Ï”‚ğ—˜—pj
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)‰¡R OracleƒT[ƒo•‰‰×ŒyŒ¸‘ÎôiSQL‚ÉƒoƒCƒ“ƒh•Ï”‚ğ—˜—pj

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				// DEL 2007.05.10 “Œ“sj‚–Ø –¢g—pŠÖ”‚ÌƒRƒƒ“ƒg‰»
				//				logFileClose();
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			// DEL 2007.05.10 “Œ“sj‚–Ø –¢g—pŠÖ”‚ÌƒRƒƒ“ƒg‰»
			//// ADD 2005.05.23 “Œ“sj¬“¶’J ‰ïˆõƒ`ƒFƒbƒN’Ç‰Á START
			//			// ‰ïˆõƒ`ƒFƒbƒN
			//			sRet[0] = userCheck2(conn2, sUser);
			//			if(sRet[0].Length > 0)
			//			{
			//				disconnect2(sUser, conn2);
			//				logFileClose();
			//				return sRet;
			//			}
			//// ADD 2005.05.23 “Œ“sj¬“¶’J ‰ïˆõƒ`ƒFƒbƒN’Ç‰Á END

			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				cmdQuery
					// MOD 2005.05.11 “Œ“sj‚–Ø ORA-03113‘ÎôH START
					//					= "SELECT —X•Ö”Ô†, TRIM(“s“¹•{Œ§–¼), TRIM(s‹æ’¬‘º–¼), TRIM(’¬ˆæ–¼), \n"
					//					+        "“s“¹•{Œ§‚b‚c || s‹æ’¬‘º‚b‚c || ‘åš’ÊÌ‚b‚c \n"
					//					+   " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i \n";
					= GET_BYPOSTCODE2J_SELECT;
				// MOD 2005.05.11 “Œ“sj‚–Ø ORA-03113‘ÎôH START
				if(s—X•Ö”Ô†.Length == 7)
				{
					cmdQuery += " WHERE —X•Ö”Ô† = '" + s—X•Ö”Ô† + "' \n";
				}
				else
				{
					cmdQuery += " WHERE —X•Ö”Ô† LIKE '" + s—X•Ö”Ô† + "%' \n";
				}
				cmdQuery +=    " AND íœ‚e‚f = '0' \n";

				// MOD-S 2012.09.06 COA)‰¡R OracleƒT[ƒo•‰‰×ŒyŒ¸‘ÎôiSQL‚ÉƒoƒCƒ“ƒh•Ï”‚ğ—˜—pj
				//OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				logWriter(sUser, INF_SQL, "###ƒoƒCƒ“ƒhŒãi‘z’èj###\n" + cmdQuery);	//C³‘O‚ÌUPDATE•¶‚ğƒƒOo—Í

				cmdQuery = GET_BYPOSTCODE2J_SELECT;
				if(s—X•Ö”Ô†.Length == 7)
				{
					cmdQuery += " WHERE —X•Ö”Ô† = :p_YuubinNo \n";
				}
				else
				{
					cmdQuery += " WHERE —X•Ö”Ô† LIKE :p_YuubinNo \n";
				}
				cmdQuery +=    " AND íœ‚e‚f = '0' \n";

				wk_opOraParam = new OracleParameter[1];
				if(s—X•Ö”Ô†.Length == 7)
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s—X•Ö”Ô†, ParameterDirection.Input);
				}
				else
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s—X•Ö”Ô†+"%", ParameterDirection.Input);
				}

				OracleDataReader	reader = CmdSelect(sUser, conn2, cmdQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)‰¡R OracleƒT[ƒo•‰‰×ŒyŒ¸‘ÎôiSQL‚ÉƒoƒCƒ“ƒh•Ï”‚ğ—˜—pj

				if (reader.Read())
				{
					// MOD 2005.05.11 “Œ“sj‚–Ø ORA-03113‘ÎôH START
					//					sRet[1] = reader.GetString(0);	// —X•Ö”Ô†
					//					sRet[2] = reader.GetString(1)	// “s“¹•{Œ§–¼
					//							+ reader.GetString(2)	// s‹æ’¬‘º–¼
					//							+ reader.GetString(3);	// ’¬ˆæ–¼
					//					sRet[3] = reader.GetString(4);	// ZŠ‚b‚c
					sRet[1] = reader.GetString(0).Trim();	// —X•Ö”Ô†
					sRet[2] = reader.GetString(1).Trim()	// “s“¹•{Œ§–¼
						+ reader.GetString(2).Trim()	// s‹æ’¬‘º–¼
						+ reader.GetString(3).Trim();	// ’¬ˆæ–¼
					sRet[3] = reader.GetString(4).Trim()	// “s“¹•{Œ§‚b‚c
						+ reader.GetString(5).Trim()	// s‹æ’¬‘º‚b‚c
						+ reader.GetString(6).Trim();	// ‘åš’ÊÌ‚b‚c
					// MOD 2005.05.11 “Œ“sj‚–Ø ORA-03113‘ÎôH END
					sRet[0] = "³íI—¹";
				}
				else
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
				}
				// ADD 2007.04.28 “Œ“sj‚–Ø ƒIƒuƒWƒFƒNƒg‚Ì”jŠü START
				disposeReader(reader);
				reader = null;
				// ADD 2007.04.28 “Œ“sj‚–Ø ƒIƒuƒWƒFƒNƒg‚Ì”jŠü END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				// ADD 2007.04.28 “Œ“sj‚–Ø ƒIƒuƒWƒFƒNƒg‚Ì”jŠü START
				conn2 = null;
				// ADD 2007.04.28 “Œ“sj‚–Ø ƒIƒuƒWƒFƒNƒg‚Ì”jŠü END
				// DEL 2007.05.10 “Œ“sj‚–Ø –¢g—pŠÖ”‚ÌƒRƒƒ“ƒg‰»
				//				logFileClose();

			}
			return sRet;
		}

		/*********************************************************************
		 * ƒAƒbƒvƒ[ƒhƒf[ƒ^’Ç‰Á‚Q ‰¤q‘Î‰
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA‰×ól‚b‚c...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2otodoke\Service1.asmx.cs(1209):
		*/ 
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i \n"
			;

		[WebMethod]
		public String[] otodoke_Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "‚¨“ÍæƒAƒbƒvƒ[ƒhƒf[ƒ^’Ç‰Á‚QŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try{
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow+1] = "";

					string[] sData = sList[iRow].Split(',');
					string sZŠ‚b‚c = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						sZŠ‚b‚c = sData[21];
					}
// ADD 2008.06.11 kcl)X–{ ’…“XƒR[ƒhŒŸõ•û–@‚Ì•ÏX START
					string s“Áê‚b‚c = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s“Áê‚b‚c = sData[19];
//					}
// ADD 2008.06.11 kcl)X–{ ’…“XƒR[ƒhŒŸõ•û–@‚Ì•ÏX END

//					sData[15] = sData[15].TrimEnd();
//					if(sData[15].Length == 0){
//						sRet[iRow+1] = "—X–¢";//–¢İ’è
//						continue;
//					}
//					if(sData[15].Length != 7){
//						sRet[iRow+1] = "—XŒ…";//Œ…”‚ÉŒë‚è‚ª‚ ‚éê‡
//						continue;
//					}

					//—X•Ö”Ô†ƒ}ƒXƒ^‚Ì‘¶İƒ`ƒFƒbƒN
					OracleDataReader reader;
					string cmdQuery = "";
					cmdQuery = INS_UPLOADDATA2_SELECT1
							+ "WHERE —X•Ö”Ô† = '" + sData[15] + "' \n"
//•Û—¯ MOD 2010.04.13 “Œ“sj‚–Ø —X•Ö”Ô†‚ªíœ‚³‚ê‚½‚ÌáŠQ‘Î‰ START
							+ "AND íœ‚e‚f = '0' \n"
//•Û—¯ MOD 2010.04.13 “Œ“sj‚–Ø —X•Ö”Ô†‚ªíœ‚³‚ê‚½‚ÌáŠQ‘Î‰ END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow+1] = sData[15];//ŠY“–ƒf[ƒ^–³‚µ
						reader.Close();
						disposeReader(reader);
						reader = null;
						continue;
					}
					reader.Close();

					cmdQuery
						= "SELECT íœ‚e‚f \n"
						+   "FROM ‚r‚l‚O‚Q‰×ól \n"
						+  "WHERE ‰ïˆõ‚b‚c = '" + sData[0] + "' \n"
						+    "AND •”–å‚b‚c = '" + sData[1] + "' \n"
						+    "AND ‰×ól‚b‚c = '" + sData[2] + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string síœ‚e‚f = "";
					while (reader.Read()){
						síœ‚e‚f = reader.GetString(0);
						iCnt++;
					}
					reader.Close();

					if(iCnt == 1){
						//’Ç‰Á
						cmdQuery 
							= "INSERT INTO ‚r‚l‚O‚Q‰×ól \n"
							+ "VALUES ( "
							+           "'" + sData[0] + "', "
							+           "'" + sData[1] + "', \n"
							+           "'" + sData[2] + "', "
							+           "'" + sData[3] + "', \n"
							+           "'" + sData[4] + "', "
							+           "'" + sData[5] + "', \n"
							+           "'" + sData[6] + "', "
							+           "'" + sData[7] + "', \n"
							+           "'" + sData[8] + "', "
							+           "'" + sData[9] + "', \n"
							+           "'" + sData[10] + "', "
							+           "'" + sData[11] + "', \n"
							+           "'" + sData[12] + "', "
							+           "'" + sData[13] + "', \n"
							+           "'" + sData[14] + "', "
							+           "'" + sData[15] + "', \n"
							+           "'" + sZŠ‚b‚c + "', "
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)X–{ ’…“XƒR[ƒhŒŸõ•û–@‚Ì•ÏX START
//							+           "' ', \n" //“Áê‚b‚c
							+           "'" + s“Áê‚b‚c + "', \n"
// ADD 2008.06.11 kcl)X–{ ’…“XƒR[ƒhŒŸõ•û–@‚Ì•ÏX END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							+           "' ', \n"
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'‚¨“Íæ', \n"
							+           "'" + sData[20] + "')"
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//ã‘‚«XV
						cmdQuery
							= "UPDATE ‚r‚l‚O‚Q‰×ól \n"
							+    "SET “d˜b”Ô†‚P = '" + sData[3] + "' "
							+       ",“d˜b”Ô†‚Q = '" + sData[4] + "' \n"
							+       ",“d˜b”Ô†‚R = '" + sData[5] + "' "
							+       ",‚e‚`‚w”Ô†‚P = '" + sData[6] + "' \n"
							+       ",‚e‚`‚w”Ô†‚Q = '" + sData[7] + "' "
							+       ",‚e‚`‚w”Ô†‚R = '" + sData[8] + "' \n"
							+       ",ZŠ‚P = '" + sData[9] + "' "
							+       ",ZŠ‚Q = '" + sData[10] + "' \n"
							+       ",ZŠ‚R = '" + sData[11] + "' "
							+       ",–¼‘O‚P = '" + sData[12] + "' \n"
							+       ",–¼‘O‚Q = '" + sData[13] + "' "
							+       ",–¼‘O‚R = '" + sData[14] + "' \n"
							+       ",—X•Ö”Ô† = '" + sData[15] + "' "
							+       ",ZŠ‚b‚c = '" + sZŠ‚b‚c + "' \n"
							+       ",ƒJƒi—ªÌ = '" + sData[16] + "' "
							+       ",ˆêÄo‰×‹æ•ª = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)X–{ ’…“XƒR[ƒhŒŸõ•û–@‚Ì•ÏX START
//							+       ",“Áê‚b‚c = ' ' \n" //“Áê‚b‚c
							+       ",“Áê‚b‚c = '" + s“Áê‚b‚c + "' \n"
// ADD 2008.06.13 kcl)X–{ ’…“XƒR[ƒhŒŸõ•û–@‚Ì•ÏX END
							+       ",“ÁêŒv = '" + sData[18] + "' \n"
							+       ",ƒ[ƒ‹ƒAƒhƒŒƒX = ' ' "
							+       ",íœ‚e‚f = '0' \n"
							+       ",“o˜^‚o‚f = ' ' \n"
							;
						if(síœ‚e‚f == "1"){
							cmdQuery
								+=  ",“o˜^“ú = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",“o˜^Ò = '" + sData[20] + "' \n"
								;
						}
						cmdQuery
							+=      ",XV“ú = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",XV‚o‚f = '‚¨“Íæ' "
							+       ",XVÒ = '" + sData[20] + "' \n"
							+ "WHERE ‰ïˆõ‚b‚c = '" + sData[0] + "' \n"
							+   "AND •”–å‚b‚c = '" + sData[1] + "' \n"
							+   "AND ‰×ól‚b‚c = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "³íI—¹");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

// MOD 2010.09.08 “Œ“sj‚–Ø ‚b‚r‚uæ‹@”\‚Ì’Ç‰Á START
		/*********************************************************************
		 * ƒAƒbƒvƒ[ƒhƒf[ƒ^’Ç‰Á‚Q  ‰¤q@‚²ˆË—Šå“o˜^
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA‰×ól‚b‚c...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		/* GŠÛ‚Å‰º‚Ìs‚ÉƒJ[ƒ\ƒ‹‚ğ‚à‚Á‚Ä‚¢‚«[F10]ƒL[‚ğ‰Ÿ‚·‚ÆŒ³ƒ\[ƒX‚ªQÆ‚Å‚«‚Ü‚·
		..\is2goirai\Service1.asmx.cs(1698):
		*/
 		private static string goirai_INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM ‚b‚l‚P‚S—X•Ö”Ô†‚i \n"
			;

		private static string goirai_INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
//			= "SELECT —X•Ö”Ô† \n"
//			+ " FROM ‚b‚l‚O‚Q•”–å \n"
			= "SELECT CM02.—X•Ö”Ô† \n"
			+ ", NVL(CM01.•Û—¯ˆóü‚e‚f,'0') \n"
			+ " FROM ‚b‚l‚O‚Q•”–å CM02 \n"
			+ " LEFT JOIN ‚b‚l‚O‚P‰ïˆõ CM01 \n"
			+ " ON CM02.‰ïˆõ‚b‚c = CM01.‰ïˆõ‚b‚c \n"
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
			;

		private static string goirai_INS_UPLOADDATA2_SELECT3
			= "SELECT 1 \n"
			+ " FROM ‚r‚l‚O‚S¿‹æ \n"
			;

		[WebMethod]
		public String[] goirai_Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "‚²ˆË—ŠåƒAƒbƒvƒ[ƒhƒf[ƒ^’Ç‰Á‚QŠJn");

			OracleConnection conn2 = null;
			string[] sRet = new string[(sList.Length*2) + 1];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();
			OracleDataReader reader;
			string cmdQuery = "";

			sRet[0] = "";
			try{
				string s•”—X•Ö”Ô† = "";
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
				string sd—Ê“ü—Í§Œä = "0";
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow*2+1] = "";
					sRet[iRow*2+2] = "";

					string[] sData = sList[iRow].Split(',');
					if(sData.Length != 21){
						throw new Exception("ƒpƒ‰ƒ[ƒ^’·ƒGƒ‰[["+sData.Length+"]");
					}

					string s‰ïˆõ‚b‚c   = sData[0];
					string s•”–å‚b‚c   = sData[1];
					string s‰×‘—l‚b‚c = sData[2];
					string s—X•Ö”Ô†   = sData[12];
					string s¿‹æ‚b‚c = sData[17];
					string s¿‹æ•”‰Û = sData[18];

					if(iRow == 0){
						//•”–åƒ}ƒXƒ^‚Ì‘¶İƒ`ƒFƒbƒN
						cmdQuery = goirai_INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
//								+ "WHERE ‰ïˆõ‚b‚c = '" + s‰ïˆõ‚b‚c + "' \n"
//								+ "AND •”–å‚b‚c = '" + s•”–å‚b‚c + "' \n"
//								+ "AND íœ‚e‚f = '0' \n"
								+ "WHERE CM02.‰ïˆõ‚b‚c = '" + s‰ïˆõ‚b‚c + "' \n"
								+ "AND CM02.•”–å‚b‚c = '" + s•”–å‚b‚c + "' \n"
								+ "AND CM02.íœ‚e‚f = '0' \n"
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
								;

						reader = CmdSelect(sUser, conn2, cmdQuery);
						if(!reader.Read()){
							reader.Close();
							disposeReader(reader);
							reader = null;
							throw new Exception("ƒZƒNƒVƒ‡ƒ“‚ª‘¶İ‚µ‚Ü‚¹‚ñ");
						}
						s•”—X•Ö”Ô† = reader.GetString(0).TrimEnd();
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
						sd—Ê“ü—Í§Œä = reader.GetString(1).TrimEnd();
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
						reader.Close();
						disposeReader(reader);
						reader = null;
					}

					//—X•Ö”Ô†ƒ}ƒXƒ^‚Ì‘¶İƒ`ƒFƒbƒN
					cmdQuery = goirai_INS_UPLOADDATA2_SELECT1
// MOD 2010.09.29 “Œ“sj‚–Ø —X•Ö”Ô†(__)‘Î‰i–Šù‘¶ƒoƒO‚¾‚ª“±“üj START
//							+ "WHERE —X•Ö”Ô† = '" + s—X•Ö”Ô† + "' \n"
//							+ "AND íœ‚e‚f = '0' \n"
							;
							string s—X•Ö”Ô†‚P = "";
							string s—X•Ö”Ô†‚Q = "";
							if(s—X•Ö”Ô†.Length > 3){
								s—X•Ö”Ô†‚P = s—X•Ö”Ô†.Substring(0,3).Trim();
								s—X•Ö”Ô†‚Q = s—X•Ö”Ô†.Substring(3).Trim();
								s—X•Ö”Ô† = s—X•Ö”Ô†‚P + s—X•Ö”Ô†‚Q;
							}
							if(s—X•Ö”Ô†.Length == 7){
								cmdQuery += " WHERE —X•Ö”Ô† = '" + s—X•Ö”Ô† + "' \n";
							}else{
								cmdQuery += " WHERE —X•Ö”Ô† LIKE '" + s—X•Ö”Ô† + "%' \n";
							}
							cmdQuery += "AND íœ‚e‚f = '0' \n"
// MOD 2010.09.29 “Œ“sj‚–Ø —X•Ö”Ô†(__)‘Î‰i–Šù‘¶ƒoƒO‚¾‚ª“±“üj END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+1] = s—X•Ö”Ô†.TrimEnd();//ŠY“–ƒf[ƒ^–³‚µ
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					//¿‹æƒ}ƒXƒ^‚Ì‘¶İƒ`ƒFƒbƒN
					cmdQuery = goirai_INS_UPLOADDATA2_SELECT3
							+ "WHERE —X•Ö”Ô† = '" + s•”—X•Ö”Ô† + "' \n"
							+ "AND “¾ˆÓæ‚b‚c = '" + s¿‹æ‚b‚c + "' \n"
							+ "AND “¾ˆÓæ•”‰Û‚b‚c = '" + s¿‹æ•”‰Û + "' \n"
// MOD 2011.03.09 “Œ“sj‚–Ø ¿‹æƒ}ƒXƒ^‚ÌåƒL[‚É[‰ïˆõ‚b‚c]‚ğ’Ç‰Á START
							+ "AND ‰ïˆõ‚b‚c = '" + s‰ïˆõ‚b‚c + "' \n"
// MOD 2011.03.09 “Œ“sj‚–Ø ¿‹æƒ}ƒXƒ^‚ÌåƒL[‚É[‰ïˆõ‚b‚c]‚ğ’Ç‰Á END
 							+ "AND íœ‚e‚f = '0' \n"
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+2] = s¿‹æ‚b‚c.TrimEnd(); //ŠY“–ƒf[ƒ^–³‚µ
						if(s¿‹æ•”‰Û.TrimEnd().Length > 0){
							sRet[iRow*2+2] += "-" + s¿‹æ•”‰Û.TrimEnd();
						}
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;
					
					//ƒGƒ‰[‚ª‚ ‚ê‚ÎAŸ‚Ìs
					if(sRet[iRow*2+1].Length != 0 || sRet[iRow*2+2].Length != 0){
						continue;
					}

					cmdQuery
						= "SELECT íœ‚e‚f \n"
						+   "FROM ‚r‚l‚O‚P‰×‘—l \n"
						+  "WHERE ‰ïˆõ‚b‚c = '" + s‰ïˆõ‚b‚c + "' \n"
						+    "AND •”–å‚b‚c = '" + s•”–å‚b‚c + "' \n"
						+    "AND ‰×‘—l‚b‚c = '" + s‰×‘—l‚b‚c + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string síœ‚e‚f = "";
					while (reader.Read()){
						síœ‚e‚f = reader.GetString(0);
						iCnt++;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					if(iCnt == 1){
						//’Ç‰Á
						cmdQuery 
							= "INSERT INTO ‚r‚l‚O‚P‰×‘—l \n"
							+ "VALUES ( \n"
							+  "'" + sData[0] + "', "		//‰ïˆõ‚b‚c
							+  "'" + sData[1] + "', \n"		//•”–å‚b‚c
							+  "'" + sData[2] + "', \n"		//‰×‘—l‚b‚c

							+  "'" + sData[17] + "', "		//“¾ˆÓæ‚b‚c
							+  "'" + sData[18] + "', \n"	//“¾ˆÓæ•”‰Û‚b‚c
							+  "'" + sData[3] + "', "		//“d˜b”Ô†
							+  "'" + sData[4] + "', "
							+  "'" + sData[5] + "', \n"
							+  "' ', "						//‚e‚`‚w”Ô†
							+  "' ', "
							+  "' ', \n"
							+  "'" + sData[6] + "', "		//ZŠ
							+  "'" + sData[7] + "', "
							+  "'" + sData[8] + "', \n"
							+  "'" + sData[9] + "', "		//–¼‘O
							+  "'" + sData[10] + "', "
							+  "'" + sData[11] + "', \n"
							+  "'" + sData[12] + "', "		//—X•Ö”Ô†
							+  "'" + sData[13] + "', \n"	//ƒJƒi—ªÌ
							+  " " + sData[14] + " , "		//Ë”
							+  " " + sData[15] + " , \n"	//d—Ê
							+  "' ', "						//‰×D‹æ•ª
							+  "'" + sData[16] + "', \n"	//ƒ[ƒ‹ƒAƒhƒŒƒX
							+  "'0', "
							+  "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+  "'" + sData[19] + "', "
							+  "'" + sData[20] + "', \n"
							+  "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+  "'" + sData[19] + "', "
							+  "'" + sData[20] + "' \n"
							+  ") "
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//ã‘‚«XV
						cmdQuery
							= "UPDATE ‚r‚l‚O‚P‰×‘—l \n"
							+    "SET “¾ˆÓæ‚b‚c = '" + sData[17] + "' \n"
							+       ",“¾ˆÓæ•”‰Û‚b‚c = '" + sData[18] + "' \n"
							+       ",“d˜b”Ô†‚P = '" + sData[3] + "' \n"
							+       ",“d˜b”Ô†‚Q = '" + sData[4] + "' \n"
							+       ",“d˜b”Ô†‚R = '" + sData[5] + "' \n"
							+       ",‚e‚`‚w”Ô†‚P = ' ' \n"
							+       ",‚e‚`‚w”Ô†‚Q = ' ' \n"
							+       ",‚e‚`‚w”Ô†‚R = ' ' \n"
							+       ",ZŠ‚P = '" + sData[6] + "' \n"
							+       ",ZŠ‚Q = '" + sData[7] + "' \n"
							+       ",ZŠ‚R = '" + sData[8] + "' \n"
							+       ",–¼‘O‚P = '" + sData[9] + "' \n"
							+       ",–¼‘O‚Q = '" + sData[10] + "' \n"
							+       ",–¼‘O‚R = '" + sData[11] + "' \n"
							+       ",—X•Ö”Ô† = '" + sData[12] + "' "
							+       ",ƒJƒi—ªÌ = '" + sData[13] + "' "
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
							;
						if(sd—Ê“ü—Í§Œä == "1"){
							cmdQuery = cmdQuery
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
							+       ",Ë” = "+ sData[14] +" "
							+       ",d—Ê = "+ sData[15] +" "
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
							;
						}
						cmdQuery = cmdQuery
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
							+       ",‰×D‹æ•ª = ' ' "
							+       ",\"ƒ[ƒ‹ƒAƒhƒŒƒX\" = '"+ sData[16] +"' "
							+       ",íœ‚e‚f = '0' \n"
							;
						if(síœ‚e‚f == "1"){
							cmdQuery
								+=  ",“o˜^“ú = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",“o˜^‚o‚f = '" + sData[19] + "' "
								+   ",“o˜^Ò = '" + sData[20] + "' \n"
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
								;
							if(sd—Ê“ü—Í§Œä != "1"){
								cmdQuery = cmdQuery
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
								+   ",Ë” = "+ sData[14] +" "
								+   ",d—Ê = "+ sData[15] +" \n"
								;
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä START
							}
// MOD 2011.05.06 “Œ“sj‚–Ø ‚¨‹q—l‚²‚Æ‚Éd—Ê“ü—Í§Œä END
						}
						cmdQuery
							+=      ",XV“ú = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",XV‚o‚f = '" + sData[19] + "' "
							+       ",XVÒ = '" + sData[20] + "' \n"
							+ "WHERE ‰ïˆõ‚b‚c = '" + sData[0] + "' \n"
							+   "AND •”–å‚b‚c = '" + sData[1] + "' \n"
							+   "AND ‰×‘—l‚b‚c = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "³íI—¹");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 “Œ“sj‚–Ø ‚b‚r‚uæ‹@”\‚Ì’Ç‰Á END

// ADD 2015.05.01 BEVAS) ‘O“c CM14J—X•Ö”Ô†‘¶İƒ`ƒFƒbƒN END
	}

}
