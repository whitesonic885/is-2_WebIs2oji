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
	// 修正履歴
	//--------------------------------------------------------------------------
	// 2010.12.14 ACT）垣原 新規作成
	//--------------------------------------------------------------------------
	// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 
	//--------------------------------------------------------------------------
	// MOD 2011.01.06 東都）高木 郵便番号の印刷 
	// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない 
	// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 
	// MOD 2011.04.13 東都）高木 重量入力不可対応
	// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 
	// MOD 2011.06.01 東都）高木 ＳＱＬの調整 
	// MOD 2011.07.14 東都）高木 記事行の追加 
	// MOD 2011.10.06 東都）高木 出荷データの印刷ログの追加 
	// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 
	//--------------------------------------------------------------------------
	// MOD 2015.05.01 BEVAS) 前田 CM14J郵便番号存在チェック
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
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();

			connectService();
		}

		#region コンポーネント デザイナで生成されたコード 
		
		//Web サービス デザイナで必要です。
		private IContainer components = null;
				
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
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
		 * 発店取得
		 * 引数：荷送人ＣＤ
		 * 戻値：ステータス、店所ＣＤ、店所名、都道府県ＣＤ、市区町村ＣＤ、大字通称ＣＤ
		 *
		 *********************************************************************/
		private static string GET_HATUTEN3_SELECT
			= "SELECT CM14.店所ＣＤ \n"
			+  " FROM ＣＭ０２部門 CM02 \n"
			+      ", ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
			;

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2init\Service1.asmx.cs(3062):
		*/
		[WebMethod]
		public String[] Get_hatuten3(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "発店取得３開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_HATUTEN3_SELECT);
				sbQuery.Append(" WHERE CM02.会員ＣＤ = '" + sKcode + "' \n");
				sbQuery.Append(" AND CM02.部門ＣＤ = '" + sBcode + "' \n");
				sbQuery.Append(" AND CM02.郵便番号 = CM14.郵便番号 \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				if(reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();

					if (sRet[1].Equals("999")) // 王子運送対応
					{
						sRet[0] = "指定した住所は、配達不可能エリアです";
					}
					else
					{
						sRet[0] = "正常終了";
					}
				}
				else
				{
					sRet[0] = "利用者の集荷店取得に失敗しました";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 部門マスタ検索
		 * 引数：会員ＣＤ、部門ＣＤ
		 * 戻値：ステータス、部門ＣＤ、部門名、出力順、店所名、更新日時
		 *
		 * 参照元：会員マスタ.cs 2回
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(717):
		*/
		[WebMethod]
		public string[] Sel_Section(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "部門マスタ検索開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[19];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT CM02.部門ＣＤ "
					+      ", CM02.部門名 "
					+      ", CM02.出力順 "
					+      ", CM02.郵便番号 "
					+      ", NVL(CM10.店所名, ' ') "
					+      ", CM02.設置先住所１ "
					+      ", CM02.設置先住所２ "
					+      ", CM02.更新日時 \n"
					+      ", CM02.サーマル台数 \n"
					+      ", NVL(CM06.シリアル番号１,' ') \n"
					+      ", NVL(CM06.状態１,' ') \n"
					+      ", NVL(CM06.シリアル番号２,' ') \n"
					+      ", NVL(CM06.状態２,' ') \n"
					+      ", NVL(CM06.シリアル番号３,' ') \n"
					+      ", NVL(CM06.状態３,' ') \n"
					+      ", NVL(CM06.シリアル番号４,' ') \n"
					+      ", NVL(CM06.状態４,' ') \n"
					+      ", NVL(CM06.使用料,0) \n"
					+  " FROM ＣＭ０２部門 CM02 \n"
					+      " LEFT JOIN ＣＭ０６部門拡張 CM06 \n"
					+      " ON CM02.会員ＣＤ = CM06.会員ＣＤ \n"
					+      " AND CM02.部門ＣＤ = CM06.部門ＣＤ \n"
					+  " LEFT JOIN ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
					+    " ON CM02.郵便番号 = CM14.郵便番号 "
					+  " LEFT JOIN ＣＭ１０店所 CM10 \n"
					+    " ON CM14.店所ＣＤ = CM10.店所ＣＤ "
					+   " AND CM10.削除ＦＧ = '0' \n"
					+ " WHERE CM02.会員ＣＤ = '" + sKey[0] + "' \n"
					+   " AND CM02.部門ＣＤ = '" + sKey[1] + "' \n"
					+   " AND CM02.削除ＦＧ = '0' \n"
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
					sRet[0] = "該当データがありません";
				else
					sRet[0] = "正常終了";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 請求先マスタ一覧取得
		 * 引数：店所ＣＤ
		 * 戻値：ステータス、一覧（郵便番号、得意先ＣＤ）...
		 *
		 * 参照元：請求先マスタ.cs 現在未使用
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(2797):
		*/
		[WebMethod]
		public string[] Get_Claim(string[] sUser, string sKey)
		{
			logWriter(sUser, INF, "請求先マスタ一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(SM04.郵便番号) || '|' "
					+     "|| TRIM(SM04.得意先ＣＤ)     || '|' "
					+     "|| TRIM(SM04.得意先部課ＣＤ) || '|' "
					+     "|| TRIM(SM04.得意先部課名)   || '|' "
					+     "|| TRIM(SM04.会員ＣＤ) || '|' "
					+     "|| NVL(CM01.会員名, ' ')  || '|' "
					+     "|| TO_CHAR(SM04.更新日時) || '|' \n"
					+  " FROM ＣＭ１４郵便番号Ｊ CM14 " // 王子運送対応
					+      ", ＳＭ０４請求先 SM04 \n"
					+  " LEFT JOIN ＣＭ０１会員 CM01 \n"
					+    " ON SM04.会員ＣＤ = CM01.会員ＣＤ "
					+    "AND '0' = CM01.削除ＦＧ \n"
					+ " WHERE CM14.店所ＣＤ = '" + sKey + "' \n"
					+   " AND CM14.郵便番号 = SM04.郵便番号 \n"
					+   " AND SM04.削除ＦＧ = '0' \n"
					+ " ORDER BY SM04.会員ＣＤ "
					+          ",SM04.得意先ＣＤ "
					+          ",SM04.得意先部課ＣＤ \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 請求先マスタ一覧取得２
		 * 引数：店所ＣＤ、会員ＣＤ
		 * 戻値：ステータス、一覧（郵便番号、得意先ＣＤ）...
		 *
		 * 参照元：請求先マスタ.cs
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(2908):
		*/
		[WebMethod]
		public string[] Get_Claim2(string[] sUser, string sTensyo, string sKaiin)
		{
			logWriter(sUser, INF, "請求先マスタ一覧取得２開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(SM04.郵便番号) || '|' "
					+     "|| TRIM(SM04.会員ＣＤ) || '|' "
					+     "|| NVL(TRIM(CM01.会員名), ' ')  || '|' "
					+     "|| TRIM(SM04.得意先ＣＤ)     || '|' "
					+     "|| TRIM(SM04.得意先部課ＣＤ) || '|' "
					+     "|| TRIM(SM04.得意先部課名)   || '|' "
					+     "|| TO_CHAR(SM04.更新日時) || '|' \n"
					+  " FROM ＣＭ１４郵便番号Ｊ CM14 " // 王子運送対応
					+      ", ＳＭ０４請求先 SM04 \n"
					+  " LEFT JOIN ＣＭ０１会員 CM01 \n"
					+    " ON SM04.会員ＣＤ = CM01.会員ＣＤ "
					+    "AND '0' = CM01.削除ＦＧ \n"
					+ " WHERE CM14.店所ＣＤ = '" + sTensyo + "' \n";

				if(sKaiin.Length > 0)
				{
					cmdQuery += "AND  SM04.会員ＣＤ = '" + sKaiin + "' \n";
				}
				cmdQuery
					+=  " AND CM14.郵便番号 = SM04.郵便番号 \n"
					+   " AND SM04.削除ＦＧ = '0' \n"
					+   " AND CM01.管理者区分 IN ('1','3','4') \n" // 1:管理者 3:王子一般 4:王子営業所
					+ " ORDER BY SM04.会員ＣＤ "
					+          ",SM04.得意先ＣＤ "
					+          ",SM04.得意先部課ＣＤ \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 郵便番号マスタ取得
		 * 引数：郵便番号
		 * 戻値：ステータス、店所名
		 *
		 * 参照元：会員マスタ.cs
		 * 参照元：請求先マスタ.cs
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(3668):
		*/
		[WebMethod]
		public string[] Sel_Postcode(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "郵便番号マスタ検索開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[4]{"","","",""};

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT NVL(CM10.店所名, ' '), \n"
					+ " TRIM(CM14.都道府県名) || TRIM(CM14.市区町村名) || TRIM(CM14.町域名) \n"
					+ ", CM14.店所ＣＤ \n"
					+  " FROM ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
					+  " LEFT JOIN ＣＭ１０店所 CM10 \n"
					+    " ON CM14.店所ＣＤ = CM10.店所ＣＤ "
					+    "AND CM10.削除ＦＧ = '0' \n"
					+ " WHERE CM14.郵便番号 = '" + sKey[0] + "' \n"
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
					sRet[0] = "該当データがありません";
				}
				else
				{
					if (sRet[3].Equals("999")) // 王子運送対応
					{
						sRet[0] = "指定した住所は、配達不可能エリアです";
					}
					else
					{
						sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 郵便番号マスタ取得
		 * 引数：郵便番号
		 * 戻値：ステータス、住所、店所正式名、店所ＣＤ
		 *
		 * 参照元：会員加入.cs		[]	
		 * 参照元：店所情報.cs		[]	
		 * 参照元：請求先マスタ.cs	[]	
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(7318):
		*/
		[WebMethod]
		public string[] Sel_Postcode1(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "郵便番号マスタ検索開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT NVL(CM10.店所名, ' '), \n"
					+ " TRIM(CM14.都道府県名) || TRIM(CM14.市区町村名) || TRIM(CM14.町域名),NVL(TRIM(CM10.店所正式名), ' '),TRIM(CM14.店所ＣＤ) \n"
					+  " FROM ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
					+  " LEFT JOIN ＣＭ１０店所 CM10 \n"
					+    " ON CM14.店所ＣＤ = CM10.店所ＣＤ "
					+    "AND CM10.削除ＦＧ = '0' \n"
					+ " WHERE CM14.郵便番号 = '" + sKey[0] + "' \n"
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
					sRet[0] = "該当データがありません";
				}
				else
				{
					if (sRet[4].Equals("999")) // 王子運送対応
					{
						sRet[0] = "指定した住所は、配達不可能エリアです";
					}
					else
					{
						sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * ログイン認証
		 * 引数：会員ＣＤ、利用者ＣＤ、パスワード
		 * 戻値：ステータス、会員ＣＤ、会員名、利用者ＣＤ、利用者名
		 *
		 *********************************************************************/
		private static string SET_LOGIN_SELECT1
			= "SELECT CM01.会員ＣＤ, \n"
			+ " CM01.会員名, \n"
			+ " CM04.利用者ＣＤ, \n"
			+ " CM04.利用者名 \n"
			+ ", CM01.管理者区分 \n"
			+ ", NVL(CM14.店所ＣＤ,' ') \n"
			+ " FROM ＣＭ０１会員 CM01, \n"
			+ " ＣＭ０２部門 CM02, \n"
			+ " ＣＭ１４郵便番号Ｊ CM14, \n" // 王子運送対応
			+ " ＣＭ０４利用者 CM04 \n";

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(4657):
		*/
		[WebMethod]
		public string[] Set_login(string[] sUser, string[] sKey) 
		{
			logWriter(sUser, INF, "ログイン認証開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[7];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= SET_LOGIN_SELECT1
					+ " WHERE CM01.会員ＣＤ = '" + sKey[0] + "' \n"
					+   " AND CM01.会員ＣＤ = CM04.会員ＣＤ \n"
					+   " AND CM04.利用者ＣＤ = '" + sKey[1] + "' \n"
					+   " AND CM04.パスワード = '" + sKey[2] + "' \n"
					+   " AND CM01.使用開始日 <= TO_CHAR(SYSDATE,'YYYYMMDD') \n"
					+   " AND CM01.使用終了日 >= TO_CHAR(SYSDATE,'YYYYMMDD') \n"
					+   " AND CM01.管理者区分 IN ('1','4') \n" // 1:管理者 4:王子営業所
					+   " AND CM01.削除ＦＧ = '0' \n"
					+   " AND CM04.削除ＦＧ = '0' \n"
					+   " AND CM04.会員ＣＤ = CM02.会員ＣＤ \n"
					+   " AND CM04.部門ＣＤ = CM02.部門ＣＤ \n"
					+   " AND           '0' = CM02.削除ＦＧ \n"
					+   " AND CM02.郵便番号 = CM14.郵便番号(+) \n"
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
					if (sRet[6].Equals("999")) // 王子運送対応
					{
						sRet[0] = "指定した住所は、配達不可能エリアです";
					}
					else
					{
						sRet[0] = "正常終了";
					}
				}
				else
				{
					sRet[0] = "該当データがありません";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 会員情報取得（ＣＳＶ出力用）
		 * 引数：会員ＣＤ、使用開始日（開始、終了）、使用終了日（開始、終了）、
		 *		 利用者登録日（開始、終了）
		 * 戻値：ステータス、会員ＣＤ、会員名、使用開始日...
		 *
		 * 参照元：会員情報ＣＳＶ出力.cs
		 *********************************************************************/
		private static string GET_KAIINCSV_SELECT
			= "SELECT R.会員ＣＤ,NVL(K.会員名,' '),NVL(K.使用開始日,' '),NVL(K.使用終了日,' '), \n"
			+       " R.部門ＣＤ,NVL(B.部門名,' '),NVL(Y.店所ＣＤ,' '),NVL(T.店所名,' '), \n"
			+       " NVL(B.設置先住所１,' '),NVL(B.設置先住所２,' '), \n"
			+       " R.利用者ＣＤ,R.\"パスワード\",R.利用者名,SUBSTR(R.登録日時,1,8) \n"
			+       " ,NVL(B.\"サーマル台数\",'0')\n"
			+      ", NVL(CM06.シリアル番号１,' '), DECODE(CM06.状態１,'1 ','返品','2 ','不良品','3 ','不明','4 ','その他','5 ','発送中',' ') \n"
			+      ", NVL(CM06.シリアル番号２,' '), DECODE(CM06.状態２,'1 ','返品','2 ','不良品','3 ','不明','4 ','その他','5 ','発送中',' ') \n"
			+      ", NVL(CM06.シリアル番号３,' '), DECODE(CM06.状態３,'1 ','返品','2 ','不良品','3 ','不明','4 ','その他','5 ','発送中',' ') \n"
			+      ", NVL(CM06.シリアル番号４,' '), DECODE(CM06.状態４,'1 ','返品','2 ','不良品','3 ','不明','4 ','その他','5 ','発送中',' ') \n"
			+      ", DECODE(K.管理者区分,'1','管理者','3','王子一般','4','王子営業所', K.管理者区分) \n"
			+      ", DECODE(K.記事連携ＦＧ,'0',' ','1','運賃非表示', K.記事連携ＦＧ) \n"
			+      ", K.登録日時, K.更新日時 \n"
			+      ", B.組織ＣＤ, B.郵便番号, NVL(CM06.使用料,0) \n"
			+      ", DECODE(CM06.会員申込管理番号,NULL,' ',0,' ',TO_CHAR(CM06.会員申込管理番号)) \n"
			+      ", B.登録日時, B.更新日時 \n"
			+      ", R.荷送人ＣＤ \n"
			+      ", DECODE(R.権限１,' ',' ','1','ラベル印刷禁止', R.権限１) \n"
			+      ", R.\"認証エラー回数\" \n"
			+      ", R.登録ＰＧ \n"
			+      ", R.登録日時, R.更新日時 \n"
			+ " FROM ＣＭ０１会員 K,ＣＭ０２部門 B,ＣＭ０４利用者 R,ＣＭ１０店所 T,ＣＭ１４郵便番号Ｊ Y \n" // 王子運送対応
			+ " ,ＣＭ０６部門拡張 CM06 \n"
			;

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(5304):
		*/
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "会員情報ＣＳＶ出力用取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbQuery2 = new StringBuilder(1024);
			try
			{
				sbQuery.Append(" WHERE R.会員ＣＤ = K.会員ＣＤ \n");
				sbQuery.Append(" AND R.会員ＣＤ = B.会員ＣＤ \n");
				sbQuery.Append(" AND R.部門ＣＤ = B.部門ＣＤ \n");
				sbQuery.Append(" AND B.郵便番号 = Y.郵便番号(+) \n");
				sbQuery.Append(" AND Y.店所ＣＤ = T.店所ＣＤ(+) \n");
				sbQuery.Append(" AND R.削除ＦＧ = '0' \n");
				sbQuery.Append(" AND '0' = K.削除ＦＧ \n");
				sbQuery.Append(" AND '0' = B.削除ＦＧ \n");
				sbQuery.Append(" AND '0' = T.削除ＦＧ(+) \n");
				sbQuery.Append(" AND R.会員ＣＤ = CM06.会員ＣＤ(+) \n");
				sbQuery.Append(" AND R.部門ＣＤ = CM06.部門ＣＤ(+) \n");
				sbQuery.Append(" AND K.管理者区分 IN ('3','4') \n"); // 3:王子一般 4:王子営業所

				
				if(sData[0].Length > 0 && sData[1].Length > 0)
					sbQuery.Append(" AND R.会員ＣＤ  BETWEEN '"+ sData[0] + "' AND '"+ sData[1] +"' \n");
				else
				{
					if(sData[0].Length > 0 && sData[1].Length == 0)
						sbQuery.Append(" AND R.会員ＣＤ = '"+ sData[0] + "' \n");
				}

				if(sData[2].Length > 0 && sData[3].Length > 0)
					sbQuery.Append(" AND K.使用開始日  BETWEEN '"+ sData[2] + "' AND '"+ sData[3] +"' \n");
				else
				{
					if(sData[2].Length > 0 && sData[3].Length == 0)
						sbQuery.Append(" AND K.使用開始日 = '"+ sData[2] + "' \n");
				}

				if(sData[4].Length > 0 && sData[5].Length > 0)
					sbQuery.Append(" AND K.使用終了日  BETWEEN '"+ sData[4] + "' AND '"+ sData[5] +"' \n");
				else
				{
					if(sData[4].Length > 0 && sData[5].Length == 0)
						sbQuery.Append(" AND K.使用終了日 = '"+ sData[4] + "' \n");
				}

				if(sData[6].Length > 0 && sData[7].Length > 0)
					sbQuery.Append(" AND SUBSTR(R.登録日時,1,8)  BETWEEN '"+ sData[6] + "' AND '"+ sData[7] +"' \n");
				else
				{
					if(sData[6].Length > 0 && sData[7].Length == 0)
						sbQuery.Append(" AND SUBSTR(R.登録日時,1,8) = '"+ sData[6] + "' \n");
				}
				sbQuery.Append(" ORDER BY R.会員ＣＤ,R.利用者ＣＤ ");


				OracleDataReader reader;

				sbQuery2.Append(GET_KAIINCSV_SELECT);
				sbQuery2.Append(sbQuery);
				reader = CmdSelect(sUser, conn2, sbQuery2);

				StringBuilder sbData = new StringBuilder(1024);
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
					sbData.Append(sDbl + sSng + reader.GetString(0).Trim() + sDbl);				// 会員ＣＤ
					sbData.Append(sKanma + sDbl + reader.GetString(1).Trim() + sDbl);			// 会員名
					sbData.Append(sKanma + sDbl + reader.GetString(2).Trim() + sDbl);			// 使用開始日
					sbData.Append(sKanma + sDbl + reader.GetString(3).Trim() + sDbl);			// 使用終了日
					sbData.Append(sKanma + sDbl + reader.GetString(23).TrimEnd() + sDbl);		// 管理者区分
					sbData.Append(sKanma + sDbl + reader.GetString(24).TrimEnd() + sDbl);		// 運賃非表示（記事連携ＦＧ）
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(25).ToString().TrimEnd() + sDbl); // 登録日時
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(26).ToString().TrimEnd() + sDbl); // 更新日時
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(4).Trim() + sDbl);	// 部門ＣＤ
					sbData.Append(sKanma + sDbl + reader.GetString(5).Trim() + sDbl);			// 部門名
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(6).Trim() + sDbl);	// 管理店所ＣＤ
					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);			// 管理店所名
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).Trim() + sDbl);	// 設置先住所１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).Trim() + sDbl);	// 設置先住所２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(27).TrimEnd() + sDbl);		// Ver.（組織ＣＤ）
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(28).TrimEnd() + sDbl);		// 郵便番号
					sbData.Append(sKanma + sDbl + reader.GetDecimal(29).ToString().TrimEnd() + sDbl); // 使用料
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(30).TrimEnd() + sDbl); // 会員申込管理番号
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(31).ToString().TrimEnd() + sDbl); // 登録日時
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(32).ToString().TrimEnd() + sDbl); // 更新日時
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).Trim() + sDbl);	// 利用者ＣＤ
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).Trim() + sDbl);	// パスワード
					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl       );	// 利用者名
					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);			// 利用者登録日
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(33).TrimEnd() + sDbl); // 荷送人ＣＤ
					sbData.Append(sKanma + sDbl + reader.GetString(34).TrimEnd() + sDbl);		 // ラベル印刷禁止
					sbData.Append(sKanma + sDbl + reader.GetDecimal(35).ToString().TrimEnd() + sDbl); // 認証エラー回数
					sbData.Append(sKanma + sDbl + reader.GetString(36).TrimEnd() + sDbl); // パスワード更新日（登録ＰＧ）
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(37).ToString().TrimEnd() + sDbl); // 登録日時
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(38).ToString().TrimEnd() + sDbl); // 更新日時
					sbData.Append(sKanma + sDbl + reader.GetDecimal(14) + sDbl);			// サーマル台数
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).Trim() + sDbl);	// シリアル番号１
					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);			// 状態１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).Trim() + sDbl);	// シリアル番号２
					sbData.Append(sKanma + sDbl + reader.GetString(18).Trim() + sDbl);			// 状態２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(19).Trim() + sDbl);	// シリアル番号３
					sbData.Append(sKanma + sDbl + reader.GetString(20).Trim() + sDbl);			// 状態３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(21).Trim() + sDbl);	// シリアル番号４
					sbData.Append(sKanma + sDbl + reader.GetString(22).Trim() + sDbl);			// 状態４

					sList.Add(sbData);
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 会員マスタ取得
		 * 引数：会員ＣＤ
		 * 戻値：ステータス、会員ＣＤ、会員名、使用開始日、管理者区分、使用終了日
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(7634):
		*/
		[WebMethod]
		public string[] Sel_MemberTn(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "会員マスタ検索開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[8];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT CM01.会員ＣＤ "
					+       ",CM01.会員名 "
					+       ",CM01.使用開始日 "
					+       ",CM01.管理者区分 "
					+       ",CM01.使用終了日 "
					+       ",CM01.更新日時 \n"
					+       ",CM01.記事連携ＦＧ \n"
					+  " FROM ＣＭ０１会員 CM01\n"
					+  "     ,ＣＭ０２部門 CM02\n"
					+  "     ,ＣＭ１４郵便番号Ｊ CM14\n" // 王子運送対応
					+ " WHERE CM01.会員ＣＤ = '" + sKey[0] + "' \n"
					+    "AND CM01.削除ＦＧ = '0' \n"
					+    "AND CM01.会員ＣＤ = CM02.会員ＣＤ \n"
					+    "AND CM02.削除ＦＧ = '0' \n"
					+    "AND CM14.郵便番号 = CM02.郵便番号 \n"
					+    "AND CM14.店所ＣＤ = '" + sKey[1] + "' \n";

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
					sRet[0] = "該当データがありません";
				else
					sRet[0] = "正常終了";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 会員マスタ取得
		 * 引数：会員ＣＤ
		 * 戻値：ステータス、会員ＣＤ、会員名、使用開始日、管理者区分、使用終了日
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(127):
		*/
		[WebMethod]
		public string[] Sel_Member(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "会員マスタ検索開始");

			OracleConnection conn2 = null;
			// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
			//			string[] sRet = new string[8];
			string[] sRet = new string[9];
			// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT 会員ＣＤ "
					+       ",会員名 "
					+       ",使用開始日 "
					+       ",管理者区分 "
					+       ",使用終了日 "
					+       ",更新日時 \n"
					+       ",記事連携ＦＧ \n"
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					+       ",保留印刷ＦＧ \n"
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					+  " FROM ＣＭ０１会員 \n"
					// MOD 2011.06.01 東都）高木 ＳＱＬの調整 START
					//					+ " WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
					//					+ " OR 会員ＣＤ = 'J" + sKey[0] + "' \n" // 王子運送対応
					//					+    "AND 削除ＦＧ = '0' \n"
					+ " WHERE ( 会員ＣＤ = '" + sKey[0] + "' \n"
					+ "  OR 会員ＣＤ = 'J" + sKey[0] + "' ) \n" // 王子運送対応
					+ " AND 削除ＦＧ = '0' \n"
					+ " ORDER BY 会員ＣＤ \n"
					;
				// MOD 2011.06.01 東都）高木 ＳＱＬの調整 END

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
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					sRet[8] = reader.GetString(7);
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if(iCnt == 1) 
					sRet[0] = "該当データがありません";
				else
					sRet[0] = "正常終了";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 会員マスタ一覧取得２
		 * 引数：会員ＣＤ、会員名
		 * 戻値：ステータス、会員ＣＤ、会員名
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(7741):
		*/
		[WebMethod]
		public string[] Get_MemberTn(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "会員マスタ一覧取得２開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT 会員.会員情報 from ( "
					+ "SELECT '|' "
					+     "|| TRIM(CM01.会員ＣＤ) || '|' "
					+     "|| TRIM(CM01.会員名) || '|' "
					+     "|| TRIM(使用終了日) || '|' "
					+     "|| TO_CHAR(SYSDATE,'YYYYMMDD') || '|' "
					+     "会員情報 \n"
					+  " FROM ＣＭ０１会員 CM01\n"
					+  "     ,ＣＭ０２部門 CM02 \n"
					+  "     ,ＣＭ１４郵便番号Ｊ CM14 \n"; // 王子運送対応
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.会員ＣＤ = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.会員ＣＤ LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.会員名 LIKE '%" + sKey[1] + "%' \n";
				}

				cmdQuery += " AND CM01.管理者区分 IN ('1','3','4') \n"; // 1:管理者 3:王子一般 4:王子営業所
				cmdQuery += " AND CM01.削除ＦＧ = '0' \n";

				cmdQuery += " AND CM01.会員ＣＤ = CM02.会員ＣＤ \n";
				cmdQuery += " AND CM02.削除ＦＧ = '0' \n";
				cmdQuery += " AND CM14.郵便番号 = CM02.郵便番号 \n";
				if (sKey[2].Trim().Length != 0)
				{
					cmdQuery += " AND CM14.店所ＣＤ = '" + sKey[2] + "' \n";
				}
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.使用終了日 >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += " ORDER BY CM01.会員ＣＤ \n";
				cmdQuery += " ) 会員 GROUP BY 会員.会員情報 \n";

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
					sRet[0] = "該当データがありません";
				}
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 会員マスタ一覧取得３
		 * 引数：会員ＣＤ、会員名
		 * 戻値：ステータス、会員ＣＤ、会員名
		 *
		 * 参照元：会員検索２.cs
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(7881):
		*/
		[WebMethod]
		public string[] Get_MemberTn3(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "会員マスタ一覧取得３開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' "
					+     "|| TRIM(CM01.会員ＣＤ) || '|' "
					+     "|| TRIM(CM01.会員名) || '|' "
					+     "|| TRIM(使用終了日) || '|' "
					+     "|| TO_CHAR(SYSDATE,'YYYYMMDD') || '|' "
					+     "会員情報 \n"
					+     ", CM01.会員ＣＤ kcd \n"
					+  " FROM ＣＭ０１会員 CM01\n";
				cmdQuery += "     ,ＣＭ０２部門 CM02 \n";
				cmdQuery += "     ,ＣＭ１４郵便番号Ｊ CM14 \n"; // 王子運送対応
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.会員ＣＤ = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.会員ＣＤ LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.会員名 LIKE '%" + sKey[1] + "%' \n";
				}
				cmdQuery += " AND CM01.管理者区分 IN ('1','3','4') \n"; // 1:管理者 3:王子一般 4:王子営業所
				cmdQuery += " AND CM01.削除ＦＧ = '0' \n";

				cmdQuery += " AND CM01.会員ＣＤ = CM02.会員ＣＤ \n"
					+ " AND CM02.削除ＦＧ = '0' \n"
					+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
					;
				if (sKey[2].Trim().Length != 0)
					cmdQuery += " AND CM14.店所ＣＤ = '" + sKey[2] + "' \n";
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.使用終了日 >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += "UNION \n";
				cmdQuery += "SELECT '|' "
					+ "|| TRIM(CM01.会員ＣＤ) || '|' "
					+ "|| TRIM(CM01.会員名) || '|' 会員情報 \n"
					+ ", CM01.会員ＣＤ \n"
					+ " FROM ＣＭ０１会員 CM01 \n"
					+ "     ,ＣＭ０５会員扱店 CM05 \n";
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.会員ＣＤ = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.会員ＣＤ LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.会員名 LIKE '%" + sKey[1] + "%' \n";
				}
				cmdQuery += " AND CM01.管理者区分 IN ('1','3','4') \n"; // 3:王子一般 4:王子営業所
				cmdQuery += " AND CM01.削除ＦＧ = '0' \n"
					+ " AND CM01.会員ＣＤ = CM05.会員ＣＤ \n"
					+ " AND CM05.削除ＦＧ = '0' \n";
				if (sKey[2].Trim().Length != 0)
					cmdQuery += " AND CM05.店所ＣＤ = '" + sKey[2] + "' \n";
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.使用終了日 >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
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
					sRet[0] = "該当データがありません";
				}
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * ご依頼主一覧取得（global対応）
		 * 引数：会員ＣＤ、荷送人名、荷送人ＣＤ、店所ＣＤ
		 * 戻値：ステータス、一覧（名前１、住所１、荷送人ＣＤ）...
		 *
		 * 参照元：ご依頼主検索２.cs
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(8633):
		*/
		[WebMethod]
		public string[] Get_Goirainusi2(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "ご依頼主一覧取得２開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' "
					+     "|| TRIM(SM01.会員ＣＤ) || '|' "
					+     "|| TRIM(CM01.会員名) || '|' "
					+     "|| TRIM(CM02.部門名) || '|' "
					+     "|| TRIM(SM01.荷送人ＣＤ) || '|' "
					+     "|| TRIM(SM01.名前１) || '|' "
					+     "|| TRIM(SM01.住所１) || '|' "
					+     "|| TRIM(SM01.部門ＣＤ) || '|' \n"
					+    ",CM01.会員ＣＤ kcd \n"
					+  " FROM ＳＭ０１荷送人 SM01"
					+       ",ＣＭ０２部門 CM02"
					+       ",ＣＭ１４郵便番号Ｊ CM14" // 王子運送対応
					+       ",ＣＭ０１会員 CM01 \n"
					+ " WHERE SM01.会員ＣＤ =  CM01.会員ＣＤ \n";
				if (sKey[0].Length == 10)
				{
					cmdQuery += " AND SM01.会員ＣＤ = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " AND SM01.会員ＣＤ LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Length == 12)
				{
					cmdQuery += " AND SM01.荷送人ＣＤ = '" + sKey[1] + "' \n";
				}
				else
				{
					if (sKey[1].Length != 0)
					{
						cmdQuery += " AND SM01.荷送人ＣＤ LIKE '" + sKey[1] + "%' \n";
					}
				}
				if (sKey[2].Length != 0)
				{
					cmdQuery += " AND SM01.名前１ LIKE '%" + sKey[2] + "%' \n";
				}
				cmdQuery += " AND SM01.会員ＣＤ =  CM02.会員ＣＤ \n"
					+  " AND SM01.部門ＣＤ =  CM02.部門ＣＤ \n"
					+  " AND CM02.郵便番号 =  CM14.郵便番号 \n"
					;
				if (sKey[3].Length != 0)
				{
					cmdQuery += " AND CM14.店所ＣＤ =  '" + sKey[3] + "' \n";
				}
				cmdQuery += " AND SM01.削除ＦＧ = '0' \n"
					+  " AND CM02.削除ＦＧ = '0' \n"
					+  " AND CM01.削除ＦＧ = '0' \n";

				cmdQuery += "UNION \n";
				cmdQuery += "SELECT '|' "
					+     "|| TRIM(SM01.会員ＣＤ) || '|' "
					+     "|| TRIM(CM01.会員名) || '|' "
					+     "|| TRIM(CM02.部門名) || '|' "
					+     "|| TRIM(SM01.荷送人ＣＤ) || '|' "
					+     "|| TRIM(SM01.名前１) || '|' "
					+     "|| TRIM(SM01.住所１) || '|' "
					+     "|| TRIM(SM01.部門ＣＤ) || '|' \n"
					+    ",CM01.会員ＣＤ \n"
					+  " FROM ＳＭ０１荷送人 SM01"
					+       ",ＣＭ０２部門 CM02"
					+       ",ＣＭ０５会員扱店 CM05"
					+       ",ＣＭ０１会員 CM01 \n"
					+ " WHERE SM01.会員ＣＤ =  CM01.会員ＣＤ \n"
					+ "";
				if (sKey[0].Length == 10)
				{
					cmdQuery += " AND SM01.会員ＣＤ = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " AND SM01.会員ＣＤ LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Length == 12)
				{
					cmdQuery += " AND SM01.荷送人ＣＤ = '" + sKey[1] + "' \n";
				}
				else
				{
					if (sKey[1].Length != 0)
					{
						cmdQuery += " AND SM01.荷送人ＣＤ LIKE '" + sKey[1] + "%' \n";
					}
				}
				if (sKey[2].Length != 0)
				{
					cmdQuery += " AND SM01.名前１ LIKE '%" + sKey[2] + "%' \n";
				}
				cmdQuery += " AND SM01.会員ＣＤ =  CM02.会員ＣＤ \n"
					+  " AND SM01.部門ＣＤ =  CM02.部門ＣＤ \n"
					+  " AND SM01.会員ＣＤ =  CM05.会員ＣＤ \n"
					;
				if (sKey[3].Length != 0)
				{
					cmdQuery += " AND CM05.店所ＣＤ =  '" + sKey[3] + "' \n";
				}
				cmdQuery += " AND SM01.削除ＦＧ = '0' \n"
					+  " AND CM02.削除ＦＧ = '0' \n"
					+  " AND CM05.削除ＦＧ = '0' \n"
					+  " AND CM01.削除ＦＧ = '0' \n";
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
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 依頼主データ取得
		 * 引数：会員ＣＤ、部門ＣＤ、荷送人ＣＤ、店所ＣＤ
		 * 戻値：ステータス、カナ略称、電話番号、郵便番号、住所、名前、重量
		 *		 メールアドレス、得意先ＣＤ、得意先部課ＣＤ、更新日時
		 *********************************************************************/
		private static string GET_SIRAINUSI_SELECT1
			= "SELECT SM01.名前１ \n"
			+ " FROM ＳＭ０１荷送人 SM01 \n"
			+ ", ＣＭ０２部門 CM02 \n"
			+ ", ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
			+ "";

		private static string GET_SIRAINUSI_SELECT2
			= "SELECT CM02.部門名 \n"
			+ " FROM ＣＭ０２部門 CM02 \n"
			+ ", ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
			+ "";

		private static string GET_SIRAINUSI_SELECT3
			= "SELECT CM01.会員名 \n"
			+ " FROM ＣＭ０１会員 CM01 \n"
			+ ", ＣＭ０２部門 CM02 \n"
			+ ", ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
			+ "";
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(10314):
		*/
		/*
				[WebMethod]
				public String[] Get_Sirainusi(string[] sUser, string sKCode, string sBCode, string sICode, string sTCode)
				{
					logWriter(sUser, INF, "依頼主情報取得開始");

					OracleConnection conn2 = null;
					string[] sRet = new string[4]{"","","",""};

					// ＤＢ接続
					conn2 = connect2(sUser);
					if(conn2 == null)
					{
						sRet[0] = "ＤＢ接続エラー";
						return sRet;
					}
					try
					{
						string cmdQuery = "";
						OracleDataReader reader;

						if(sKCode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT3
								+ " WHERE CM01.会員ＣＤ = '" + sKCode + "' \n"
								+ " AND CM01.削除ＦＧ = '0' \n"
								+ " AND CM01.会員ＣＤ = CM02.会員ＣＤ \n"
								+ " AND CM02.削除ＦＧ = '0' \n"
								+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
								+ "";

							//店所ＣＤが設定されている時
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[1]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
							if(sBCode.Length > 0)
							{
								cmdQuery = GET_SIRAINUSI_SELECT2
									+ " WHERE CM02.会員ＣＤ = '" + sKCode + "' \n"
									+ " AND CM02.部門ＣＤ = '" + sBCode + "' \n"
									+ " AND CM02.削除ＦＧ = '0' \n"
									+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
									+ "";

								//店所ＣＤが設定されている時
								if(sTCode.Length > 0)
								{
									cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
								}

								reader = CmdSelect(sUser, conn2, cmdQuery);

								if(reader.Read()) sRet[2]  = reader.GetString(0).Trim();
								disposeReader(reader);
								reader = null;

								if(sICode.Length > 0)
								{
									cmdQuery = GET_SIRAINUSI_SELECT1
										+ " WHERE SM01.会員ＣＤ = '" + sKCode + "' \n"
										+ " AND SM01.部門ＣＤ = '" + sBCode + "' \n"
										+ " AND SM01.荷送人ＣＤ = '" + sICode + "' \n"
										+ " AND SM01.削除ＦＧ = '0' \n"
										+ " AND SM01.会員ＣＤ = CM02.会員ＣＤ \n"
										+ " AND SM01.部門ＣＤ = CM02.部門ＣＤ \n"
										+ " AND CM02.削除ＦＧ = '0' \n"
										+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
										+ "";

									//店所ＣＤが設定されている時
									if(sTCode.Length > 0)
									{
										cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
									}

									reader = CmdSelect(sUser, conn2, cmdQuery);

									if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
									disposeReader(reader);
									reader = null;
								}
							}
							else
							{
								//部門ＣＤが未入力の場合
								if(sICode.Length > 0)
								{
									cmdQuery = GET_SIRAINUSI_SELECT1
										+ " WHERE SM01.会員ＣＤ = '" + sKCode + "' \n"
										+ " AND SM01.荷送人ＣＤ = '" + sICode + "' \n"
										+ " AND SM01.削除ＦＧ = '0' \n"
										+ " AND SM01.会員ＣＤ = CM02.会員ＣＤ \n"
										+ " AND SM01.部門ＣＤ = CM02.部門ＣＤ \n"
										+ " AND CM02.削除ＦＧ = '0' \n"
										+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
										+ "";

									//店所ＣＤが設定されている時
									if(sTCode.Length > 0)
									{
										cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
									}

									reader = CmdSelect(sUser, conn2, cmdQuery);

									if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
									disposeReader(reader);
									reader = null;
								}
							}
						}

						sRet[0] = "正常終了";
						logWriter(sUser, INF, sRet[0]);
					}
					catch (OracleException ex)
					{
						sRet[0] = chgDBErrMsg(sUser, ex);
					}
					catch (Exception ex)
					{
						sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 依頼主情報取得２
		 * 引数：ユーザー、会員ＣＤ、部門ＣＤ、荷送人ＣＤ、店所ＣＤ
		 * 戻値：依頼主情報
		 *
		 * 参照元：出荷照会.cs
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(10479):
		*/
		[WebMethod]
		public String[] Get_Sirainusi2(string[] sUser, string sKCode, string sBCode, string sICode, string sTCode)
		{
			logWriter(sUser, INF, "依頼主情報取得２開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[4]{"","","",""};

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			try
			{
				string cmdQuery = "";
				OracleDataReader reader;

				if(sKCode.Length > 0)
				{
					cmdQuery = GET_SIRAINUSI_SELECT3
						+ " WHERE CM01.会員ＣＤ = '" + sKCode + "' \n"
						+ " AND CM01.削除ＦＧ = '0' \n"
						+ " AND CM01.会員ＣＤ = CM02.会員ＣＤ \n"
						+ " AND CM02.削除ＦＧ = '0' \n"
						+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
						+ "";

					//店所ＣＤが設定されている時
					if(sTCode.Length > 0)
					{
						cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
					}
					//店所ＣＤが設定されている時
					if (sTCode.Length > 0) 
					{
						cmdQuery += "UNION \n";
						cmdQuery += "SELECT CM01.会員名 \n"
							+ " FROM ＣＭ０１会員 CM01 \n"
							+ "     ,ＣＭ０５会員扱店 CM05 \n"
							+ " WHERE CM01.会員ＣＤ = '" + sKCode + "' \n"
							+ " AND CM01.削除ＦＧ = '0' \n"
							+ " AND CM01.会員ＣＤ = CM05.会員ＣＤ \n"
							+ " AND CM05.削除ＦＧ = '0' \n"
							+ " AND CM05.店所ＣＤ = '" + sTCode + "' \n";
					}

					reader = CmdSelect(sUser, conn2, cmdQuery);

					if(reader.Read()) sRet[1]  = reader.GetString(0).Trim();
					disposeReader(reader);
					reader = null;

					if(sBCode.Length > 0)
					{
						cmdQuery = GET_SIRAINUSI_SELECT2
							+ " WHERE CM02.会員ＣＤ = '" + sKCode + "' \n"
							+ " AND CM02.部門ＣＤ = '" + sBCode + "' \n"
							+ " AND CM02.削除ＦＧ = '0' \n"
							+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
							+ "";

						//店所ＣＤが設定されている時
						if(sTCode.Length > 0)
						{
							cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
						}

						//店所ＣＤが設定されている時
						if (sTCode.Length > 0) 
						{
							cmdQuery += "UNION \n";
							cmdQuery += "SELECT CM02.部門名 \n"
								+ " FROM ＣＭ０２部門 CM02 \n"
								+ "     ,ＣＭ０５会員扱店 CM05 \n"
								+ " WHERE CM02.会員ＣＤ = '" + sKCode + "' \n"
								+ " AND CM02.部門ＣＤ = '" + sBCode + "' \n"
								+ " AND CM02.削除ＦＧ = '0' \n"
								+ " AND CM02.会員ＣＤ = CM05.会員ＣＤ \n"
								+ " AND CM05.削除ＦＧ = '0' \n"
								+ " AND CM05.店所ＣＤ = '" + sTCode + "' \n";
						}

						reader = CmdSelect(sUser, conn2, cmdQuery);

						if(reader.Read()) sRet[2]  = reader.GetString(0).Trim();
						disposeReader(reader);
						reader = null;

						if(sICode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT1
								+ " WHERE SM01.会員ＣＤ = '" + sKCode + "' \n"
								+ " AND SM01.部門ＣＤ = '" + sBCode + "' \n"
								+ " AND SM01.荷送人ＣＤ = '" + sICode + "' \n"
								+ " AND SM01.削除ＦＧ = '0' \n"
								+ " AND SM01.会員ＣＤ = CM02.会員ＣＤ \n"
								+ " AND SM01.部門ＣＤ = CM02.部門ＣＤ \n"
								+ " AND CM02.削除ＦＧ = '0' \n"
								+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
								+ "";

							//店所ＣＤが設定されている時
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
							}

							//店所ＣＤが設定されている時
							if (sTCode.Length > 0) 
							{
								cmdQuery += "UNION \n";
								cmdQuery += "SELECT SM01.名前１ \n"
									+ " FROM ＳＭ０１荷送人 SM01 \n"
									+ "     ,ＣＭ０５会員扱店 CM05 \n"
									+ " WHERE SM01.会員ＣＤ = '" + sKCode + "' \n"
									+ " AND SM01.部門ＣＤ = '" + sBCode + "' \n"
									+ " AND SM01.荷送人ＣＤ = '" + sICode + "' \n"
									+ " AND SM01.削除ＦＧ = '0' \n"
									+ " AND SM01.会員ＣＤ = CM05.会員ＣＤ \n"
									+ " AND CM05.削除ＦＧ = '0' \n"
									+ " AND CM05.店所ＣＤ = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
						}
					}
					else
					{
						//部門ＣＤが未入力の場合
						if(sICode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT1
								+ " WHERE SM01.会員ＣＤ = '" + sKCode + "' \n"
								+ " AND SM01.荷送人ＣＤ = '" + sICode + "' \n"
								+ " AND SM01.削除ＦＧ = '0' \n"
								+ " AND SM01.会員ＣＤ = CM02.会員ＣＤ \n"
								+ " AND SM01.部門ＣＤ = CM02.部門ＣＤ \n"
								+ " AND CM02.削除ＦＧ = '0' \n"
								+ " AND CM02.郵便番号 = CM14.郵便番号 \n"
								+ "";

							//店所ＣＤが設定されている時
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.店所ＣＤ = '" + sTCode + "' \n";
							}

							//店所ＣＤが設定されている時
							if (sTCode.Length > 0) 
							{
								cmdQuery += "UNION \n";
								cmdQuery += "SELECT SM01.名前１ \n"
									+ " FROM ＳＭ０１荷送人 SM01 \n"
									+ "     ,ＣＭ０５会員扱店 CM05 \n"
									+ " WHERE SM01.会員ＣＤ = '" + sKCode + "' \n"
									+ " AND SM01.荷送人ＣＤ = '" + sICode + "' \n"
									+ " AND SM01.削除ＦＧ = '0' \n"
									+ " AND SM01.会員ＣＤ = CM05.会員ＣＤ \n"
									+ " AND CM05.削除ＦＧ = '0' \n"
									+ " AND CM05.店所ＣＤ = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
						}
					}
				}

				sRet[0] = "正常終了";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 出荷印刷データ取得
		 * 引数：会員ＣＤ、部門ＣＤ、登録日、ジャーナルＮＯ
		 * 戻値：ステータス、荷受人ＣＤ、電話番号、住所...
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2print\Service1.asmx.cs(101):
		*/
		[WebMethod]
		public String[] Get_InvoicePrintData(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "出荷印刷データ取得開始");

			OracleConnection conn2 = null;
			// MOD 2011.01.06 東都）高木 郵便番号の印刷 START
			//			string[] sRet = new string[40];
			// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
			//			string[] sRet = new string[41];
			// MOD 2011.07.14 東都）高木 記事行の追加 START
			//			string[] sRet = new string[42];
			// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 START
			//			string[] sRet = new string[45];
			string[] sRet = new string[46];
			// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 END
			// MOD 2011.07.14 東都）高木 記事行の追加 END
			// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
			// MOD 2011.01.06 東都）高木 郵便番号の印刷 END
			// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 START
			string s利用者部門店所ＣＤ = (sKey.Length >  4) ? sKey[ 4] : "";
			// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 END
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			decimal d才数 = 0;
			string s郵便番号 = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT ");
				sbQuery.Append(" ST01.荷受人ＣＤ ");
				sbQuery.Append(",ST01.電話番号１ ");
				sbQuery.Append(",ST01.電話番号２ ");
				sbQuery.Append(",ST01.電話番号３ ");
				sbQuery.Append(",ST01.住所１ ");
				sbQuery.Append(",ST01.住所２ ");
				sbQuery.Append(",ST01.住所３ ");
				sbQuery.Append(",ST01.名前１ ");
				sbQuery.Append(",ST01.名前２ ");
				sbQuery.Append(",ST01.出荷日 ");
				sbQuery.Append(",ST01.送り状番号 ");
				sbQuery.Append(",ST01.郵便番号 ");
				sbQuery.Append(",ST01.着店ＣＤ ");
				sbQuery.Append(",NVL(CM14.店所ＣＤ, ST01.発店ＣＤ)");
				sbQuery.Append(",SM01.電話番号１ ");
				sbQuery.Append(",SM01.電話番号２ ");
				sbQuery.Append(",SM01.電話番号３ ");
				sbQuery.Append(",SM01.住所１ ");
				sbQuery.Append(",SM01.住所２ ");
				sbQuery.Append(",SM01.住所３ ");
				sbQuery.Append(",SM01.名前１ ");
				sbQuery.Append(",SM01.名前２ ");
				sbQuery.Append(",ST01.個数 ");
				sbQuery.Append(",ST01.重量 ");
				sbQuery.Append(",ST01.保険金額 ");
				sbQuery.Append(",ST01.指定日 ");
				sbQuery.Append(",ST01.輸送指示１ ");
				sbQuery.Append(",ST01.輸送指示２ ");
				sbQuery.Append(",ST01.品名記事１ ");
				sbQuery.Append(",ST01.品名記事２ ");
				sbQuery.Append(",ST01.品名記事３ ");
				sbQuery.Append(",ST01.元着区分 ");
				sbQuery.Append(",ST01.送り状発行済ＦＧ ");
				sbQuery.Append(",ST01.才数 \n");
				sbQuery.Append(",ST01.荷送人部署名 ");
				sbQuery.Append(",ST01.お客様出荷番号 ");
				sbQuery.Append(",ST01.輸送指示ＣＤ１ ");
				sbQuery.Append(",ST01.輸送指示ＣＤ２ ");
				sbQuery.Append(",ST01.指定日区分 ");
				sbQuery.Append(",ST01.郵便番号 ");
				sbQuery.Append(",ST01.仕分ＣＤ ");
				sbQuery.Append(",NVL(CM10.店所名, ST01.発店名)");
				sbQuery.Append(",ST01.出荷済ＦＧ ");
				// MOD 2011.01.06 東都）高木 郵便番号の印刷 START
				sbQuery.Append(",SM01.郵便番号 ");
				// MOD 2011.01.06 東都）高木 郵便番号の印刷 END
				// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				sbQuery.Append(",NVL(CM01.保留印刷ＦＧ,'0') \n");
				// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				// MOD 2011.07.14 東都）高木 記事行の追加 START
				sbQuery.Append(",ST01.品名記事４ ,ST01.品名記事５ ,ST01.品名記事６ \n");
				// MOD 2011.07.14 東都）高木 記事行の追加 END
				// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 START
				sbQuery.Append(",ST01.着店名 ");
				// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 END
				sbQuery.Append(" FROM \"ＳＴ０１出荷ジャーナル\" ST01");
				sbQuery.Append("\n");
				sbQuery.Append(" LEFT JOIN ＣＭ０２部門 CM02 \n");
				sbQuery.Append(" ON ST01.会員ＣＤ = CM02.会員ＣＤ \n");
				sbQuery.Append("AND ST01.部門ＣＤ = CM02.部門ＣＤ \n");
				sbQuery.Append(" LEFT JOIN ＣＭ１４郵便番号Ｊ CM14 \n"); // 王子運送対応
				sbQuery.Append(" ON CM02.郵便番号 = CM14.郵便番号 \n");
				sbQuery.Append(" LEFT JOIN ＣＭ１０店所 CM10 \n");
				sbQuery.Append(" ON CM14.店所ＣＤ = CM10.店所ＣＤ \n");
				// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				sbQuery.Append(" LEFT JOIN ＣＭ０１会員 CM01 \n");
				sbQuery.Append(" ON ST01.会員ＣＤ = CM01.会員ＣＤ \n");
				// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				sbQuery.Append(", \"ＳＭ０１荷送人\" SM01 \n");
				sbQuery.Append(" WHERE ST01.会員ＣＤ = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND ST01.部門ＣＤ = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND ST01.登録日 = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND ST01.ジャーナルＮＯ = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND ST01.会員ＣＤ = SM01.会員ＣＤ \n");
				sbQuery.Append(" AND ST01.部門ＣＤ = SM01.部門ＣＤ \n");
				sbQuery.Append(" AND ST01.荷送人ＣＤ = SM01.荷送人ＣＤ \n");
				sbQuery.Append(" AND ST01.削除ＦＧ = '0' \n");
				sbQuery.Append(" AND SM01.削除ＦＧ = '0' \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				int iCnt = 0;
				if (reader.Read())
				{
					string s輸送商品ＣＤ１ = reader.GetString(36).Trim();
					string s輸送商品ＣＤ２ = reader.GetString(37).Trim();
					sRet[1]  = reader.GetString(0).Trim();
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
					//					sRet[5]  = reader.GetString(4).Trim();
					//					sRet[6]  = reader.GetString(5).Trim();
					//					sRet[7]  = reader.GetString(6).Trim();
					//					sRet[8]  = reader.GetString(7).Trim();
					//					sRet[9]  = reader.GetString(8).Trim();
					sRet[5]  = reader.GetString(4).TrimEnd(); // 荷受人住所１
					sRet[6]  = reader.GetString(5).TrimEnd(); // 荷受人住所２
					sRet[7]  = reader.GetString(6).TrimEnd(); // 荷受人住所３
					sRet[8]  = reader.GetString(7).TrimEnd(); // 荷受人名前１
					sRet[9]  = reader.GetString(8).TrimEnd(); // 荷受人名前２
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[10] = reader.GetString(9).Trim();
					sRet[11] = reader.GetString(10).Trim();
					sRet[12] = reader.GetString(11).Trim();
					sRet[13] = reader.GetString(12).Trim().PadLeft(4, '0');
					sRet[14] = reader.GetString(13).Trim().PadLeft(4, '0');
					sRet[15] = reader.GetString(14).Trim();
					sRet[16] = reader.GetString(15).Trim();
					sRet[17] = reader.GetString(16).Trim();
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
					//					sRet[18] = reader.GetString(17).Trim();
					//					sRet[19] = reader.GetString(18).Trim();
					//					sRet[20] = reader.GetString(19).Trim();
					//					sRet[21] = reader.GetString(20).Trim();
					//					sRet[22] = reader.GetString(21).Trim();
					sRet[18] = reader.GetString(17).TrimEnd(); // 荷送人住所１
					sRet[19] = reader.GetString(18).TrimEnd(); // 荷送人住所２
					sRet[20] = reader.GetString(19).TrimEnd(); // 荷送人住所３
					sRet[21] = reader.GetString(20).TrimEnd(); // 荷送人名前１
					sRet[22] = reader.GetString(21).TrimEnd(); // 荷送人名前２
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[23] = reader.GetDecimal(22).ToString().Trim();
					// MOD 2011.04.13 東都）高木 重量入力不可対応 START
					//					d才数    = reader.GetDecimal(33);
					//					d才数    = d才数 * 8;
					//					if(d才数 == 0)
					//						sRet[24] = reader.GetDecimal(23).ToString().Trim();
					//					else
					//						sRet[24] = d才数.ToString().Trim();
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					if(reader.GetString(44) == "1")
					{
						d才数 = reader.GetDecimal(33) * 8;
						if(d才数 == 0)
						{
							sRet[24] = reader.GetDecimal(23).ToString().TrimEnd();
						}
						else
						{
							sRet[24] = d才数.ToString().TrimEnd();
						}
					}
					else
					{
						// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						sRet[24] = "";
						// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					}
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					// MOD 2011.04.13 東都）高木 重量入力不可対応 END
					sRet[25] = reader.GetDecimal(24).ToString().Trim();
					sRet[26] = reader.GetString(25).Trim();
					if (s輸送商品ＣＤ１.Equals("100"))
					{
						sRet[27] = reader.GetString(27).TrimEnd();
						sRet[28] = "";
					}
						// １行目と２行目が同じコードの場合、２行目を表示しない
					else if (s輸送商品ＣＤ１.Equals(s輸送商品ＣＤ２))
					{
						sRet[27] = reader.GetString(26).TrimEnd();
						sRet[28] = "";
					}
					else
					{
						sRet[27] = reader.GetString(26).TrimEnd();
						sRet[28] = reader.GetString(27).TrimEnd();
					}
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
					//					sRet[29] = reader.GetString(28).Trim();
					//					sRet[30] = reader.GetString(29).Trim();
					//					sRet[31] = reader.GetString(30).Trim();
					sRet[29] = reader.GetString(28).TrimEnd(); // 品名記事１
					sRet[30] = reader.GetString(29).TrimEnd(); // 品名記事２
					sRet[31] = reader.GetString(30).TrimEnd(); // 品名記事３
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					// パーセルの場合、"11"
					if (s輸送商品ＣＤ１.Equals("001") || s輸送商品ＣＤ１.Equals("002"))
						sRet[32] = reader.GetString(31).Trim() + "1";
					else
						sRet[32] = reader.GetString(31).Trim() + "0";
					sRet[33] = reader.GetString(32).Trim();
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
					//					sRet[34] = reader.GetString(34).Trim();
					sRet[34] = reader.GetString(34).TrimEnd(); // 担当者（部署）
					// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[35] = reader.GetString(35).Trim(); // お客様番号
					sRet[36] = reader.GetString(38).Trim();
					s郵便番号 = reader.GetString(39).Trim();
					sRet[37] = reader.GetString(40).Trim();		//仕分ＣＤ
					sRet[38] = reader.GetString(41).Trim();		//発店名
					sRet[39] = reader.GetString(42).Trim();		//出荷済ＦＧ
					// MOD 2011.01.06 東都）高木 郵便番号の印刷 START
					sRet[40] = reader.GetString(43).Trim();		//ご依頼主郵便番号
					// MOD 2011.01.06 東都）高木 郵便番号の印刷 END
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					sRet[41] = reader.GetString(44).TrimEnd();
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					// MOD 2011.07.14 東都）高木 記事行の追加 START
					sRet[42] = reader.GetString(45).TrimEnd(); // 品名記事４
					sRet[43] = reader.GetString(46).TrimEnd(); // 品名記事５
					sRet[44] = reader.GetString(47).TrimEnd(); // 品名記事６
					// MOD 2011.07.14 東都）高木 記事行の追加 END
					// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 START
					sRet[45] = reader.GetString(48).TrimEnd(); // 着店名
					// MOD 2011.12.06 東都）高木 ラベルヘッダ部に発店名・着店名を印字 END
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if (iCnt == 0)
				{
					sRet[0] = "該当データがありません";
				}
				else
				{
					sRet[0] = "正常終了";
					// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 START
					if(s利用者部門店所ＣＤ.Length == 0)
					{
						// MOD 2011.10.06 東都）高木 出荷データの印刷ログの追加 START
						logWriter(sUser, INF, "出荷印刷データ取得　利用者部門店所ＣＤ無"
							+"["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sRet[11]+"]"
							+"送り状発行済["+sRet[33]+"]出荷済["+sRet[39]+"]"
							);
						// MOD 2011.10.06 東都）高木 出荷データの印刷ログの追加 END
						return sRet;
					}
					// 利用者の部門の管轄店所ＣＤと登録者の発店ＣＤとが異なる場合
					string s発店ＣＤ = sRet[14].Trim().Substring(1, 3);
					if(!s発店ＣＤ.Equals(s利用者部門店所ＣＤ))
					{
						return sRet;
					}
					// 送り状番号がない場合には取得する
					if(sRet[11].Length == 0)
					{
						disconnect2(sUser, conn2);
						conn2 = null;

						string[] sRetInvoiceNo = Set_InvoiceNo2(sUser ,sKey, sRet, s利用者部門店所ＣＤ);
						if(sRetInvoiceNo[0].Length == 4)
						{
							//							sRet[11] = sRetInvoiceNo[1];
						}
						else
						{
							sRet[0] = sRetInvoiceNo[0];
						}
					}
					// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 END
					// MOD 2011.10.06 東都）高木 出荷データの印刷ログの追加 START
					logWriter(sUser, INF, "出荷印刷データ取得"
						+"["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sRet[11]+"]"
						+"送り状発行済["+sRet[33]+"]出荷済["+sRet[39]+"]"
						);
					// MOD 2011.10.06 東都）高木 出荷データの印刷ログの追加 END
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 送り状発行済ＦＧの更新
		 * 引数：会員ＣＤ、部門ＣＤ、登録日、ジャーナルＮＯ、送り状番号、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2print\Service1.asmx.cs(778):
		*/
		[WebMethod]
		public String[] Set_InvoiceNo(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "発行済ＦＧ更新開始");

			OracleConnection conn2 = null;
			// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 START
			//			string[] sRet = new string[1];
			string[] sRet = new string[2]{"",""};
			// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 END
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string s発店ＣＤ = "";
				string s発店名   = "";
				sbQuery.Append("SELECT NVL(CM14.店所ＣＤ, ' ') \n");
				sbQuery.Append(", NVL(CM10.店所名, ' ') \n");
				sbQuery.Append(" FROM ＣＭ０２部門 CM02 \n");
				sbQuery.Append(" LEFT JOIN ＣＭ１４郵便番号Ｊ CM14 \n"); // 王子運送対応
				sbQuery.Append(" ON CM02.郵便番号 = CM14.郵便番号 \n");
				sbQuery.Append(" LEFT JOIN ＣＭ１０店所 CM10 \n");
				sbQuery.Append(" ON CM14.店所ＣＤ = CM10.店所ＣＤ \n");
				sbQuery.Append(" WHERE CM02.会員ＣＤ = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND CM02.部門ＣＤ = '" + sKey[1] + "' \n");
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s発店ＣＤ = reader.GetString(0).Trim();
					s発店名   = reader.GetString(1).Trim();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 START
				// 送り状番号チェック
				sbQuery = new StringBuilder(1024);
				string s送り状番号 = "";
				sbQuery.Append("SELECT 送り状番号 \n");
				sbQuery.Append(" FROM  \"ＳＴ０１出荷ジャーナル\" \n");
				sbQuery.Append(" WHERE 会員ＣＤ = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND 部門ＣＤ = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND 登録日   = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND \"ジャーナルＮＯ\" = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND 削除ＦＧ = '0' \n");
				sbQuery.Append(" FOR UPDATE \n");
				reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s送り状番号 = reader.GetString(0).TrimEnd();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				if(s送り状番号.Length > 0)
				{
					// 異なる送り状番号を上書きしようとした場合
					if(s送り状番号 != sKey[4])
					{
						tran.Commit();
						sRet[0] = "エラー：他の端末で印刷中もしくは印刷済です\n"
							+ "["+s送り状番号.Substring(4)+"]";
						sRet[1] = s送り状番号;
						logWriter(sUser, INF, "送り状番号更新済["+sRet[1]+"]"
							+ " ["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sKey[4]+"]");
						return sRet;
					}
				}

				// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 END
				// 出荷ジャーナルの更新
				string cmdQuery  = "UPDATE \"ＳＴ０１出荷ジャーナル\" \n";
				cmdQuery += " SET 送り状番号 = '"  + sKey[4] + "' ";                     // 送り状番号
				// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 START
				cmdQuery +=     ",処理０１ = TO_CHAR(SYSDATE,'MMDDHH24MISS') \n"; // 送り状印刷月日時分秒
				// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 END
				cmdQuery +=     ",送り状発行済ＦＧ = '1' ";
				cmdQuery +=     ",状態 = DECODE(状態,'01','02','02','02',状態) ";
				cmdQuery +=     ",詳細状態 = DECODE(状態,'01','  ','02','  ',詳細状態) ";
				cmdQuery +=     ",更新日時 =   TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";    // 更新日時
				cmdQuery +=     ",更新ＰＧ = '出荷登録' ";                               // 更新ＰＧ
				cmdQuery +=     ",更新者 = '" + sKey[5] + "' \n";                        // 更新者
				if(s発店ＣＤ.Length > 0)
				{
					cmdQuery += ",発店ＣＤ = '" + s発店ＣＤ + "' \n";
				}
				if(s発店名.Length > 0)
				{
					cmdQuery += ",発店名 = '"   + s発店名   + "' \n";
				}
				cmdQuery += " WHERE 会員ＣＤ       = '" + sKey[0] + "' \n";
				cmdQuery +=   " AND 部門ＣＤ       = '" + sKey[1] + "' \n";
				cmdQuery +=   " AND 登録日         = '" + sKey[2] + "' \n";
				cmdQuery +=   " AND ジャーナルＮＯ = '" + sKey[3] + "' \n";
				cmdQuery +=   " AND 削除ＦＧ       = '0' \n";
				logWriter(sUser, INF, "発行済ＦＧ更新["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sKey[4]+"]");

				CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "正常終了";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
		// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 START
		/*********************************************************************
		 * 採番の更新
		 * 引数：会員ＣＤ、部門ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2print\Service1.asmx.cs(494):
		*/
		[WebMethod]
		public String[] Get_InvoiceNo(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "採番更新開始");
			
			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			//トランザクションの設定
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				decimal i登録連番     = 0;
				decimal i開始原票番号 = 0;
				decimal i終了原票番号 = 0;
				decimal i最終原票番号 = 0;
				string  s割付日       = "";
				string  s有効期限     = "";
				string  s当日日付     = "";

				string cmdQuery_am12 = "SELECT";
				cmdQuery_am12 += " AM12.登録連番 ";
				cmdQuery_am12 += ",AM12.開始原票番号 ";
				cmdQuery_am12 += ",AM12.終了原票番号 ";
				cmdQuery_am12 += ",AM12.最終原票番号 ";
				cmdQuery_am12 += ",AM12.割付日 ";
				cmdQuery_am12 += ",AM12.有効期限 ";
				cmdQuery_am12 += ",TO_CHAR(SYSDATE,'YYYYMMDD') \n";
				cmdQuery_am12 += " FROM ＡＭ１２送り状採番 AM12 \n";
				cmdQuery_am12 += " WHERE AM12.会員ＣＤ = '" + sKey[0] + "' \n";
				cmdQuery_am12 += " AND AM12.部門ＣＤ = '" + sKey[1] + "' \n";
				cmdQuery_am12 += " AND AM12.元着区分 = '" + sKey[2] + "' \n";
				cmdQuery_am12 += " AND AM12.削除ＦＧ = '0' \n";
				cmdQuery_am12 += " FOR UPDATE \n";

				OracleDataReader reader_am12 = CmdSelect(sUser, conn2, cmdQuery_am12);
				int intCnt_am12 = 0;
				sRet[1] = "";
				if (reader_am12.Read())
				{
					i登録連番     = reader_am12.GetDecimal(0);
					i開始原票番号 = reader_am12.GetDecimal(1);
					i終了原票番号 = reader_am12.GetDecimal(2);
					i最終原票番号 = reader_am12.GetDecimal(3);
					s割付日       = reader_am12.GetString(4).Trim();
					s有効期限     = reader_am12.GetString(5).Trim();
					s当日日付     = reader_am12.GetString(6).Trim();
					intCnt_am12++;

					if (i最終原票番号 < i終了原票番号 && int.Parse(s有効期限) >= int.Parse(s当日日付))
					{
						//送り状番号のセット
						sRet[1] = (i最終原票番号 + 1).ToString();
					}
				}
				disposeReader(reader_am12);
				reader_am12 = null;
				if (sRet[1].Length == 0)
				{
					//ＡＭ１２送り状採番にキーが存在しない、または
					//最終番号 >= 終了番号、または
					//有効期限 <  当日の時
					decimal i最大連番   = 0;
					decimal i開始番号   = 0;
					decimal i最終番号   = 0;
					decimal i終了番号   = 0;
					decimal i割付枚数   = 0;
					decimal i有効期限   = 0;
					decimal i有効期限年 = 0;
					decimal i有効期限月 = 0;
					decimal i有効期限日 = 0;

					//採番管理より新規原票番号枠を取得
					string cmdQuery_am10 = "SELECT";
					cmdQuery_am10 += " AM10.最大連番 ";
					cmdQuery_am10 += ",AM10.登録連番 ";
					cmdQuery_am10 += ",AM10.最終原票番号 ";
					cmdQuery_am10 += ",AM11.終了原票番号 ";
					cmdQuery_am10 += ",AM10.割付枚数 ";
					cmdQuery_am10 += ",AM10.有効期限 ";
					cmdQuery_am10 += ",TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					cmdQuery_am10 += "FROM ＡＭ１０採番管理 AM10 ";
					cmdQuery_am10 += ",ＡＭ１１送り状番号 AM11 \n";
					cmdQuery_am10 += " WHERE AM10.採番区分 = '" + sKey[2] + "' \n";
					//cmdQuery_am10 += "   AND AM10.登録連番       =  " + i登録連番;
					cmdQuery_am10 += " AND AM10.採番区分 = AM11.元着区分 \n";
					cmdQuery_am10 += " AND AM10.登録連番 = AM11.登録連番 \n";
					cmdQuery_am10 += " AND AM10.削除ＦＧ = '0' \n";
					cmdQuery_am10 += " FOR UPDATE \n";

					OracleDataReader reader_am10 = CmdSelect(sUser, conn2, cmdQuery_am10);
					int intCnt_am10 = 0;
					if (reader_am10.Read())
					{
						i最大連番     = reader_am10.GetDecimal(0);
						i登録連番     = reader_am10.GetDecimal(1);
						i最終番号     = reader_am10.GetDecimal(2);
						i終了番号     = reader_am10.GetDecimal(3);
						i割付枚数     = reader_am10.GetDecimal(4);
						i有効期限     = reader_am10.GetDecimal(5);
						s当日日付     = reader_am10.GetString(6);

						//送り状採番更新情報の取得
						i開始原票番号 = i最終番号 + 1;
						i終了原票番号 = i最終番号 + i割付枚数;
						i最終原票番号 = i開始原票番号;
						s割付日       = s当日日付;
						i有効期限年   = int.Parse(s割付日.Substring(0, 4));
						i有効期限月   = int.Parse(s割付日.Substring(4, 2)) + i有効期限 - 1;
						if (i有効期限月 > 12)
						{
							i有効期限年++;
							i有効期限月 = i有効期限月 - 12;
						}
						i有効期限日   = System.DateTime.DaysInMonth(decimal.ToInt32(i有効期限年), decimal.ToInt32(i有効期限月));
						s有効期限     = i有効期限年.ToString() + i有効期限月.ToString().PadLeft(2, '0') + i有効期限日.ToString().PadLeft(2, '0');

						//採番管理更新情報の取得
						i最終番号     = i終了原票番号;

						sRet[1] = i最終原票番号.ToString();
						intCnt_am10++;
					}
					disposeReader(reader_am10);
					reader_am10 = null;
					if (intCnt_am10 == 0)
					{
						//該当データがない場合はエラー
						throw new Exception("該当データがありません");
					}
					if (i最終番号 > i終了番号)
					{
						i登録連番++;
						if (i登録連番 > i最大連番)
						{
							i登録連番 = 1;
						}
						//送り状番号より新規原票番号枠を取得
						string cmdQuery_am11 = "SELECT";
						cmdQuery_am11 += " AM11.開始原票番号 \n";
						cmdQuery_am11 += " FROM ＡＭ１１送り状番号 AM11 \n";
						cmdQuery_am11 += " WHERE AM11.元着区分 = '" + sKey[2] + "' \n";
						cmdQuery_am11 += " AND AM11.登録連番 =  " + i登録連番 + " \n";
						cmdQuery_am11 += " AND AM11.削除ＦＧ = '0' \n";
						cmdQuery_am11 += " FOR UPDATE \n";

						OracleDataReader reader_am11 = CmdSelect(sUser, conn2, cmdQuery_am11);
						int intCnt_am11 = 0;
						if (reader_am11.Read())
						{
							i開始番号     = reader_am11.GetDecimal(0);
							//採番管理更新情報の取得
							i最終番号     = i開始番号 + i割付枚数 - 1;
							//送り状採番更新情報の取得
							i開始原票番号 = i開始番号;
							i終了原票番号 = i最終番号;
							i最終原票番号 = i開始原票番号;

							sRet[1] = i最終原票番号.ToString();
							intCnt_am11++;
						}
						disposeReader(reader_am11);
						reader_am11 = null;
						if (intCnt_am11 == 0)
						{
							//該当データがない場合はエラー
							throw new Exception("該当データがありません");
						}
					}
					// 採番管理の更新
					string updQuery_am10 = "UPDATE ＡＭ１０採番管理 \n";
					updQuery_am10 += " SET 登録連番 = " + i登録連番;
					updQuery_am10 += ", 最終原票番号 = " + i最終番号;
					updQuery_am10 += ", 更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "; // 更新日時
					updQuery_am10 += ", 更新者 = '" + sKey[3] + "' \n";                   // 更新者
					updQuery_am10 += " WHERE 採番区分 = '" + sKey[2] + "' \n";

					CmdUpdate(sUser, conn2, updQuery_am10);
				}

				string updQuery_am12 = "";
				if (intCnt_am12 == 0)
				{
					// 送り状採番の追加
					updQuery_am12  = "INSERT INTO ＡＭ１２送り状採番 \n";
					updQuery_am12 += " VALUES ('" + sKey[0] + "' ";
					updQuery_am12 +=         ",'" + sKey[1] + "' ";
					updQuery_am12 +=         ",'" + sKey[2] + "' ";
					updQuery_am12 +=         ", " + i登録連番;
					updQuery_am12 +=         ", " + i開始原票番号;
					updQuery_am12 +=         ", " + i終了原票番号;
					updQuery_am12 +=         ", " + i最終原票番号;
					updQuery_am12 +=         ",'" + s割付日 + "' ";
					updQuery_am12 +=         ",'" + s有効期限 + "' ";
					updQuery_am12 +=         ",'0' ";
					updQuery_am12 +=         ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=         ",'出荷登録' ";
					updQuery_am12 +=         ",'" + sKey[3] + "' ";
					updQuery_am12 +=         ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=         ",'出荷登録' ";
					updQuery_am12 +=         ",'" + sKey[3] + "' ";
					updQuery_am12 += " ) ";
				}
				else
				{
					// 送り状採番の更新
					updQuery_am12  = "UPDATE ＡＭ１２送り状採番 \n";
					updQuery_am12 += " SET 登録連番 =  " + i登録連番;
					updQuery_am12 +=      ", 開始原票番号 =  " + i開始原票番号;
					updQuery_am12 +=      ", 終了原票番号 =  " + i終了原票番号;
					updQuery_am12 +=      ", 最終原票番号 =  " + sRet[1];
					updQuery_am12 +=      ", 割付日 = '" + s割付日 + "'";
					updQuery_am12 +=      ", 有効期限 = '" + s有効期限 + "'";
					updQuery_am12 +=      ", 更新日時 =   TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=      ", 更新ＰＧ = '出荷登録' ";
					updQuery_am12 +=      ", 更新者 = '" + sKey[3] + "' \n";
					updQuery_am12 += " WHERE 会員ＣＤ = '" + sKey[0] + "' \n";
					updQuery_am12 +=   " AND 部門ＣＤ = '" + sKey[1] + "' \n";
					updQuery_am12 +=   " AND 元着区分 = '" + sKey[2] + "' \n";
				}
				CmdUpdate(sUser, conn2, updQuery_am12);
				tran.Commit();
				sRet[0] = "正常終了";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 送り状番号更新
		 * 引数：会員ＣＤ、部門ＣＤ、登録日、ジャーナルＮＯ、送り状番号、更新者
		 * 　　　印刷データ、利用者部門店所ＣＤ
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2print\Service1.asmx.cs(963):
		*/
		//		[WebMethod]
		private String[] Set_InvoiceNo2(string[] sUser, string[] sKey, string[] sPrintData, string sTensyo)
		{
			logWriter(sUser, INF, "送り状番号更新２開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string s送り状番号 = "";
				sbQuery.Append("SELECT 送り状番号 \n");
				sbQuery.Append(" FROM  \"ＳＴ０１出荷ジャーナル\" \n");
				sbQuery.Append(" WHERE 会員ＣＤ = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND 部門ＣＤ = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND 登録日   = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND \"ジャーナルＮＯ\" = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND 削除ＦＧ = '0' \n");
				sbQuery.Append(" FOR UPDATE \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s送り状番号 = reader.GetString(0).TrimEnd();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				if(s送り状番号.Length > 0)
				{
					tran.Commit();
					sRet[0] = "採番済み";
					sRet[1] = s送り状番号;
					logWriter(sUser, INF, "送り状番号更新２　送り状番号更新済["+s送り状番号+"]");
					return sRet;
				}
				// 送り状番号チェック
				String[] sGetKey = new string[4];
				sGetKey[0] = sKey[0];
				sGetKey[1] = sTensyo; // 利用者部門店所ＣＤ
				sGetKey[2] = sPrintData[32]; //元着区分 + "0" or "1"
				if(sPrintData[14].Substring(1, 3) == "047")
				{
					sGetKey[2] = sPrintData[32].Substring(0,1) + "G"; //元着区分 + "G"
				}
				sGetKey[3] = sUser[1];
				String[] sGetData = this.Get_InvoiceNo(sUser, sGetKey);
				if(sGetData[0].Length != 4)
				{
					tran.Commit();
					sRet[0] = sGetData[0];
					return sRet;
				}
				//送り状番号のセット
				sPrintData[11] = sGetData[1].PadLeft(14, '0');
				//チェックデジット（７で割った余り）の付加
				sPrintData[11] = sPrintData[11] + (long.Parse(sPrintData[11]) % 7).ToString();

				// 出荷ジャーナルの更新
				string cmdQuery  = "UPDATE \"ＳＴ０１出荷ジャーナル\" \n";
				cmdQuery += " SET 送り状番号 = '"  + sPrintData[11] + "' ";                     // 送り状番号
				cmdQuery += " WHERE 会員ＣＤ       = '" + sKey[0] + "' \n";
				cmdQuery +=   " AND 部門ＣＤ       = '" + sKey[1] + "' \n";
				cmdQuery +=   " AND 登録日         = '" + sKey[2] + "' \n";
				cmdQuery +=   " AND ジャーナルＮＯ = '" + sKey[3] + "' \n";
				cmdQuery +=   " AND 削除ＦＧ       = '0' \n";

				CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "正常終了";
				logWriter(sUser, INF, "送り状番号更新２　送り状番号更新["+sPrintData[11]+"]");
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
		// MOD 2011.03.25 東都）高木 送り状番号の上書き防止 END

		/*********************************************************************
		 * 発店取得
		 * 引数：荷送人ＣＤ
		 * 戻値：ステータス、店所ＣＤ、店所名、都道府県ＣＤ、市区町村ＣＤ、大字通称ＣＤ
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(1851):
		*/
		private String[] Get_hatuten(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[4];

			string cmdQuery = "SELECT Y.店所ＣＤ, T.店所名, Y.都道府県ＣＤ, Y.市区町村ＣＤ, Y.大字通称ＣＤ \n"
				+ " FROM ＣＭ０２部門 B, \n"
				+      " ＣＭ１４郵便番号Ｊ Y, \n" // 王子運送対応
				+      " ＣＭ１０店所 T \n"
				+ " WHERE B.会員ＣＤ = '" + sKcode + "' \n"
				+ " AND B.部門ＣＤ = '" + sBcode + "' \n"
				+ " AND B.削除ＦＧ = '0' \n"
				+ " AND B.郵便番号 = Y.郵便番号 \n"
				+ " AND Y.店所ＣＤ = T.店所ＣＤ \n"
				+ " AND T.削除ＦＧ = '0' \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[1] = reader.GetString(0).Trim(); // 店所ＣＤ
				sRet[2] = reader.GetString(1).Trim(); // 店所名
				sRet[3] = reader.GetString(2).Trim()  // 住所ＣＤ
					+ reader.GetString(3).Trim()
					+ reader.GetString(4).Trim();

				sRet[0] = " ";
			}
			else
			{
				sRet[0] = "発店を決められませんでした";
				sRet[1] = "0000";
				sRet[2] = " ";
				sRet[3] = " ";
			}
			disposeReader(reader);
			reader = null;
			
			return sRet;
		}

		/*********************************************************************
		 * 発店取得
		 * 引数：荷送人ＣＤ
		 * 戻値：ステータス、店所ＣＤ、店所名、都道府県ＣＤ、市区町村ＣＤ、大字通称ＣＤ
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(1932):
		*/
		[WebMethod]
		public String[] Get_hatuten2(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "発店取得開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery = "SELECT Y.店所ＣＤ, T.店所名, Y.都道府県ＣＤ, Y.市区町村ＣＤ, Y.大字通称ＣＤ \n"
					+ " FROM ＣＭ０２部門 B, \n"
					+      " ＣＭ１４郵便番号Ｊ Y, \n" // 王子運送対応
					+      " ＣＭ１０店所 T \n"
					+ " WHERE B.会員ＣＤ = '" + sKcode + "' \n"
					+ " AND B.部門ＣＤ = '" + sBcode + "' \n"
					+ " AND B.削除ＦＧ = '0' \n"
					+ " AND B.郵便番号 = Y.郵便番号 \n"
					+ " AND Y.店所ＣＤ = T.店所ＣＤ \n"
					+ " AND T.削除ＦＧ = '0' \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				if(reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim()
						+ reader.GetString(3).Trim()
						+ reader.GetString(4).Trim();

					sRet[0] = "正常終了";
				}
				else
				{
					sRet[0] = "該当データがありません";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 集約店取得
		 * 引数：会員ＣＤ、部門ＣＤ
		 * 戻値：ステータス、集約店ＣＤ
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(2070):
		*/
		private String[] Get_syuuyakuten(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[2];

			string cmdQuery = "SELECT T.集約店ＣＤ \n"
				+ " FROM ＣＭ０２部門 B,ＣＭ１０店所 T, \n"
				+        "ＣＭ１４郵便番号Ｊ Y  \n" // 王子運送対応
				+ " WHERE B.会員ＣＤ   = '" + sKcode + "' \n"
				+ "   AND B.部門ＣＤ   = '" + sBcode + "' \n"
				+ "   AND B.削除ＦＧ     = '0' \n"
				+    "AND B.郵便番号 = Y.郵便番号 \n"
				+    "AND Y.店所ＣＤ     = T.店所ＣＤ \n"
				+ "   AND T.削除ＦＧ     = '0'";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0).Trim();
			}
			else
			{
				sRet[0] = "集約店を決められませんでした";
				sRet[1] = "0000";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * 集約店取得
		 * 引数：会員ＣＤ、部門ＣＤ
		 * 戻値：ステータス、集約店ＣＤ
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(2112):
		*/
		[WebMethod]
		public String[] Get_syuuyakuten2(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "集約店取得開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery = "SELECT T.集約店ＣＤ \n"
					+ " FROM ＣＭ０２部門 B,ＣＭ１０店所 T, \n"
					+        "ＣＭ１４郵便番号Ｊ Y  \n" // 王子運送対応
					+ " WHERE B.会員ＣＤ   = '" + sKcode + "' \n"
					+ "   AND B.部門ＣＤ   = '" + sBcode + "' \n"
					+ "   AND B.削除ＦＧ     = '0' \n"
					+    "AND B.郵便番号 = Y.郵便番号 \n"
					+    "AND Y.店所ＣＤ     = T.店所ＣＤ \n"
					+ "   AND T.削除ＦＧ     = '0'";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				if(reader.Read())
				{
					sRet[0] = "正常終了";
					sRet[1] = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "該当データがありません";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 着店取得
		 * 　　ＳＭ０２荷受人、ＣＭ１４郵便番号、ＣＭ１５着店非表示、ＣＭ１９郵便住所
		 *     の４マスタを使用して着店コードを決定する。
		 * 引数：会員コード、部門コード、荷受人コード、郵便番号、住所、氏名
		 * 戻値：ステータス、店所ＣＤ、店所名、住所ＣＤ
		 *
		 * Create : 2008.06.12 kcl)森本
		 * 　　　　　　Get_tyakuten を元に作成
		 * Modify : 2008.12.24 kcl)森本
		 * 　　　　　　ＣＭ１９の検索方法を変更、および氏名からの検索を追加
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(4769):
		*/
		private String[] Get_tyakuten3(string[] sUser, OracleConnection conn2, 
			string sKaiinCode, string sBumonCode, string sNiukeCode, 
			string sYuubin, string sJuusyo, string sShimei)
		{
			string [] sRet = new string [4];		// 戻り値
			string cmdQuery;						// SQL文
			OracleDataReader reader;
			string tenCD       = string.Empty;		// 店所コード
			string tenName     = string.Empty;		// 店所名
			string juusyoCD    = string.Empty;		// 住所コード
			string address     = string.Empty;		// 住所
			string niuJuusyoCD = string.Empty;		// 荷受人マスタの住所コード

			///
			/// ＜第１段階＞
			/// 荷受人マスタの着店コードを検索
			/// 
			string niuCode = sNiukeCode.Trim();
			if (niuCode.Length > 0) 
			{
				// SQL文
				cmdQuery
					= "SELECT SM02.特殊ＣＤ, NVL(CM10.店所名, ' '), SM02.住所ＣＤ \n"
					+ "  FROM ＳＭ０２荷受人 SM02 \n"
					+ "  LEFT OUTER JOIN ＣＭ１０店所 CM10 \n"
					+ "    ON SM02.特殊ＣＤ   = CM10.店所ＣＤ \n"
					+ "   AND CM10.削除ＦＧ   = '0' \n"
					+ " WHERE SM02.会員ＣＤ   = '" + sKaiinCode + "' \n"
					+ "   AND SM02.部門ＣＤ   = '" + sBumonCode + "' \n"
					+ "   AND SM02.荷受人ＣＤ = '" + sNiukeCode + "' \n"
					+ "   AND ( LENGTH(TRIM(SM02.特殊ＣＤ)) > 0 \n"
					+ "      OR LENGTH(TRIM(SM02.住所ＣＤ)) > 0 ) \n"
					+ "   AND SM02.削除ＦＧ   = '0' \n";

				// SQL実行
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// データ取得
				if (reader.Read())
				{
					// 該当データあり

					// データ取得
					tenCD    = reader.GetString(0).Trim();		// 店所コード
					tenName  = reader.GetString(1).Trim();		// 店所名
					juusyoCD = reader.GetString(2).Trim();		// 住所コード

					if (tenCD.Length > 0) 
					{
						// 荷受人マスタの着店コードが入力されている場合

						// 住所コードの設定
						if (juusyoCD.Length == 0) 
						{
							// 荷受人マスタの住所コードが空欄の場合

							// 郵便番号マスタから取得
							string [] sResult = this.Get_juusyoCode(sUser, conn2, sYuubin);
							if (sResult[0] == " ") 
								juusyoCD = sResult[1];
						}

						// 戻り値をセット
						sRet[0] = " ";
						sRet[1] = tenCD;
						sRet[2] = tenName;
						sRet[3] = juusyoCD;

						// 終了処理
						disposeReader(reader);
						reader = null;
					
						return sRet;
					} 
					else
					{
						// 荷受人マスタに住所コードのみが入力されている場合

						// 荷受人マスタの住所コードをとっておく
						niuJuusyoCD = juusyoCD;
					}
				}

				// 終了処理
				disposeReader(reader);
				reader = null;
			}

			///
			/// ＜第２段階＞
			/// 郵便番号マスタから着店コードを検索
			///
			cmdQuery
				= "SELECT CM15.郵便番号 \n"
				+ " FROM ＣＭ１５着店非表示Ｊ CM15 \n" // 王子運送対応
				+ " WHERE CM15.郵便番号 = '" + sYuubin + "' \n"
				+ "   AND CM15.削除ＦＧ = '0' \n";

			// SQL実行
			reader = CmdSelect(sUser, conn2, cmdQuery);
			// データ取得
			if (reader.Read())
			{
				; // 郵便番号マスタは検索しない
			}
			else
			{
				// 終了処理
				disposeReader(reader);
				reader = null;
				// SQL文
				cmdQuery
					= "SELECT CM14.店所ＣＤ, CM10.店所名, CM14.都道府県ＣＤ || CM14.市区町村ＣＤ || CM14.大字通称ＣＤ \n"
					+ "  FROM ＣＭ１４郵便番号Ｊ CM14 \n" // 王子運送対応
					+ " INNER JOIN ＣＭ１０店所 CM10 \n"
					+ "    ON CM14.店所ＣＤ = CM10.店所ＣＤ \n"
					+ "   AND CM10.削除ＦＧ = '0' \n"
					+ " WHERE CM14.郵便番号 = '" + sYuubin + "' \n"
					+ "   AND LENGTH(TRIM(CM14.店所ＣＤ)) > 0 \n"
					+ "   AND CM14.削除ＦＧ = '0' \n";

				// SQL実行
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// データ取得
				if (reader.Read())
				{
					// 該当データあり

					// データ取得
					tenCD    = reader.GetString(0).Trim();		// 店所コード
					tenName  = reader.GetString(1).Trim();		// 店所名
					juusyoCD = reader.GetString(2).Trim();		// 住所コード

					// 戻り値をセット
					sRet[0] = " ";
					sRet[1] = tenCD;
					sRet[2] = tenName;
					sRet[3] = (niuJuusyoCD.Length > 0) ? niuJuusyoCD : juusyoCD;
					// ↑↑ 荷受人マスタの住所コードを優先する

					// 終了処理
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
				else 
				{
					// ＣＭ１４に該当データなし

					// 戻り値をセット
					sRet[0] = "入力されたお届け先(郵便番号)では配達店が決められませんでした";
					sRet[1] = "0000";
					sRet[2] = " ";
					sRet[3] = " ";

					// 終了処理
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
			}
			// 終了処理
			disposeReader(reader);
			reader = null;

			///
			/// ＜第３段階＞
			/// 郵便住所マスタから着店コードを検索
			/// 
			// SQL文
			cmdQuery
				= "SELECT CM19.店所ＣＤ, CM10.店所名, CM19.住所ＣＤ, CM19.住所 \n"
				+ "  FROM ＣＭ１９郵便住所Ｊ CM19 \n" // 王子運送対応
				+ " INNER JOIN ＣＭ１０店所 CM10 \n"
				+ "    ON CM19.店所ＣＤ = CM10.店所ＣＤ \n"
				+ "   AND CM10.削除ＦＧ = '0' \n"
				+ " WHERE CM19.郵便番号 = '" + sYuubin + "' \n"
				+ "   AND CM19.削除ＦＧ = '0' \n"
				+ " ORDER BY "
				+ "       LENGTH(TRIM(CM19.住所)) DESC \n"
				;

			// SQL実行
			reader = CmdSelect(sUser, conn2, cmdQuery);

			// データ取得
			while (reader.Read()) 
			{
				// 住所の取得
				address = reader.GetString(3).Trim();

				if (sShimei == null) sShimei = " ";

				// 住所・氏名のチェック
				if ((sJuusyo.IndexOf(address) >= 0) ||
					(sShimei.IndexOf(address) >= 0))
				{
					// データ取得
					tenCD    = reader.GetString(0).Trim();	// 店所コード
					tenName  = reader.GetString(1).Trim();	// 店所名
					juusyoCD = reader.GetString(2).Trim();	// 住所コード

					// 戻り値をセット
					sRet[0] = " ";
					sRet[1] = tenCD;
					sRet[2] = tenName;
					sRet[3] = (niuJuusyoCD.Length > 0) ? niuJuusyoCD : juusyoCD;
					// ↑↑ 荷受人マスタの住所コードを優先する

					// 終了処理
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
			}

			// 終了処理
			disposeReader(reader);
			reader = null;

			// 該当データ無
			sRet[0] = " ";
			sRet[1] = " ";
			sRet[2] = " ";
			sRet[3] = " ";
			
			return sRet;
		}

		/*********************************************************************
		 * 住所コード取得
		 * 　　ＣＭ１４郵便番号を使用して、郵便番号から住所コードを取得する。
		 * 引数：郵便番号
		 * 戻値：ステータス、住所ＣＤ
		 *
		 * Create : 2008.06.16 kcl)森本
		 * 　　　　　　新規作成
		 * Modify : 
		 *********************************************************************/
		private String[] Get_juusyoCode(string[] sUser, OracleConnection conn2, 
			string sYuubin)
		{
			string [] sRet = new string [2];	// 戻り値
			string cmdQuery;					// SQL文
			OracleDataReader reader;

			// SQL文
			cmdQuery
				= "SELECT CM14.都道府県ＣＤ || CM14.市区町村ＣＤ || CM14.大字通称ＣＤ \n"
				+ "  FROM ＣＭ１４郵便番号 CM14 \n"
				+ " WHERE CM14.郵便番号 = '" + sYuubin + "' \n"
				+ "   AND CM14.削除ＦＧ = '0' \n";

			// SQL実行
			reader = CmdSelect(sUser, conn2, cmdQuery);

			// データ取得
			if (reader.Read())
			{
				// 該当データあり
				sRet[0] = " ";							// ステータス
				sRet[1] = reader.GetString(0).Trim();	// 住所コード
			} 
			else
			{
				// 該当データ無
				sRet[0] = "入力された郵便番号では住所コードが決められませんでした";
				sRet[1] = " ";
			}

			// 終了処理
			disposeReader(reader);
			reader = null;
			
			return sRet;
		}

		/*********************************************************************
		 * 出荷データ更新
		 * 引数：会員ＣＤ、部門ＣＤ、出荷日...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(1161):
		*/
		[WebMethod]
		public String[] Upd_syukka2(string[] sUser, string[] sData, string sNo)
		{
			logWriter(sUser, INF, "出荷更新開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			decimal d件数;
			string s特殊計 = " ";
			try
			{
				//出荷日チェック
				string[] sSyukkabi = Get_bumonsyukka(sUser, conn2, sData[0], sData[1]);
				sRet[0] = sSyukkabi[0];
				if(sRet[0] != " ") return sRet;
				if(int.Parse(sData[2]) < int.Parse(sSyukkabi[1]))
				{
					sRet[0] = "1";
					sRet[1] = sSyukkabi[1];
					return sRet;
				}

				//荷送人ＣＤ存在チェック
				string cmdQuery
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					//					= "SELECT 得意先ＣＤ, 得意先部課ＣＤ \n"
					//					+ "  FROM ＳＭ０１荷送人 \n"
					//					+ " WHERE 会員ＣＤ   = '" + sData[0]  +"' \n"
					//					+ "   AND 部門ＣＤ   = '" + sData[1]  +"' \n"
					//					+ "   AND 荷送人ＣＤ = '" + sData[15] +"' \n"
					//					+ "   AND 削除ＦＧ   = '0'";
					= "SELECT SM01.得意先ＣＤ, SM01.得意先部課ＣＤ \n"
					+ "     , NVL(CM01.保留印刷ＦＧ,'0') \n"
					+ "  FROM ＳＭ０１荷送人 SM01 \n"
					+ "     , ＣＭ０１会員 CM01 \n"
					+ " WHERE SM01.会員ＣＤ   = '" + sData[0]  +"' \n"
					+ "   AND SM01.部門ＣＤ   = '" + sData[1]  +"' \n"
					+ "   AND SM01.荷送人ＣＤ = '" + sData[15] +"' \n"
					+ "   AND SM01.削除ＦＧ   = '0' \n"
					+ "   AND SM01.会員ＣＤ   = CM01.会員ＣＤ(+) \n"
					;
				string s重量入力制御 = "0";
				// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if(reader.Read())
				{
					d件数 = 1;
					sData[16] = reader.GetString(0);
					sData[17] = reader.GetString(1);
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					s重量入力制御 = reader.GetString(2);
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				}
				else
				{
					d件数 = 0;
				}
				disposeReader(reader);
				reader = null;

				if(d件数 == 0)
				{
					sRet[0] = "0";
				}
				else
				{
					cmdQuery
						= "SELECT SM04.得意先部課名 \n"
						+ " FROM ＣＭ０２部門 CM02 \n"
						+    " , ＳＭ０４請求先 SM04 \n"
						+ " WHERE CM02.会員ＣＤ = '" + sData[0] + "' \n"
						+  " AND CM02.部門ＣＤ = '" + sData[1] + "' \n"
						+  " AND CM02.削除ＦＧ = '0' \n"
						+  " AND SM04.会員ＣＤ = CM02.会員ＣＤ \n"
						+  " AND SM04.郵便番号 = CM02.郵便番号 \n"
						+  " AND SM04.得意先ＣＤ = '" + sData[16] + "' \n"
						+  " AND SM04.得意先部課ＣＤ = '" + sData[17] + "' \n"
						+  " AND SM04.削除ＦＧ = '0' \n"
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

					//特殊計取得
					if(sData[4] != " ")
					{
						cmdQuery
							= "SELECT NVL(特殊計,' ') \n"
							+ "  FROM ＳＭ０２荷受人 \n"
							+ " WHERE 会員ＣＤ   = '" + sData[0] +"' \n"
							+ "   AND 部門ＣＤ   = '" + sData[1] +"' \n"
							+ "   AND 荷受人ＣＤ = '" + sData[4] +"' \n"
							+ "   AND 削除ＦＧ   = '0'";

						reader = CmdSelect(sUser, conn2, cmdQuery);

						bool bRead = reader.Read();
						if(bRead == true)
							s特殊計   = reader.GetString(0);

						disposeReader(reader);
						reader = null;
						cmdQuery
							= "UPDATE ＳＭ０２荷受人 \n"
							+ " SET 登録ＰＧ = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ " WHERE 会員ＣＤ = '" + sData[0] +"' \n"
							+ " AND 部門ＣＤ   = '" + sData[1] +"' \n"
							+ " AND 荷受人ＣＤ = '" + sData[4] +"' \n"
							+ " AND 削除ＦＧ   = '0'";
						try
						{
							int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
						}
						catch(Exception)
						{
							;
						}
					}

					//着店取得
					string[] sTyakuten = Get_tyakuten3(sUser, conn2, 
						sData[0], sData[1], sData[4], 
						sData[13] + sData[14], sData[8] + sData[9] + sData[10], sData[11] + sData[12]);
					sRet[0] = sTyakuten[0];
					if(sRet[0] != " ") return sRet;
					string s着店ＣＤ = (sTyakuten[1].Length > 0) ? sTyakuten[1] : " ";
					string s着店名   = (sTyakuten[2].Length > 0) ? sTyakuten[2] : " ";
					string s住所ＣＤ = (sTyakuten[3].Length > 0) ? sTyakuten[3] : " ";

					//発店取得
					string[] sHatuten = Get_hatuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sHatuten[0];
					if(sRet[0] != " ") return sRet;
					string s発店ＣＤ = sHatuten[1];
					string s発店名   = sHatuten[2];

					//集荷店取得
					string[] sSyuyaku = Get_syuuyakuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sSyuyaku[0];
					if(sRet[0] != " ") return sRet;
					string s集約店ＣＤ = sSyuyaku[1];

					//仕分ＣＤ取得
					string s仕分ＣＤ = " ";
					if(s発店ＣＤ.Trim().Length > 0 && s着店ＣＤ.Trim().Length > 0)
					{
						string[] sRetSiwake = Get_siwake(sUser, conn2, s発店ＣＤ, s着店ＣＤ);
						s仕分ＣＤ = sRetSiwake[1];
					}
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					if(s重量入力制御 == "0")
					{
						sData[38] = "0"; // 才数
						sData[20] = "0"; // 重量
					}
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					// MOD 2011.07.14 東都）高木 記事行の追加 START
					string s品名記事４ = (sData.Length > 43) ? sData[43] : " ";
					string s品名記事５ = (sData.Length > 44) ? sData[44] : " ";
					string s品名記事６ = (sData.Length > 45) ? sData[45] : " ";
					if(s品名記事４.Length == 0) s品名記事４ = " ";
					// MOD 2011.07.14 東都）高木 記事行の追加 END

					cmdQuery 
						= "UPDATE \"ＳＴ０１出荷ジャーナル\" \n"
						+    "SET 出荷日             = '" + sData[2]  +"', \n"
						+        "お客様出荷番号     = '" + sData[3]  +"',"
						+        "荷受人ＣＤ         = '" + sData[4]  +"',"
						+        "電話番号１         = '" + sData[5]  +"', \n"
						+        "電話番号２         = '" + sData[6]  +"',"
						+        "電話番号３         = '" + sData[7]  +"',"
						+        "住所ＣＤ           = '" + s住所ＣＤ +"', \n"
						+        "住所１             = '" + sData[8]  +"',"
						+        "住所２             = '" + sData[9]  +"',"
						+        "住所３             = '" + sData[10] +"', \n"
						+        "名前１             = '" + sData[11] +"',"
						+        "名前２             = '" + sData[12] +"',"
						+        "郵便番号           = '" + sData[13] + sData[14] +"', \n"
						+        "着店ＣＤ           = '" + s着店ＣＤ +"',"
						+        "着店名             = '" + s着店名   +"',"
						+        "特殊計             = '" + s特殊計   +"', \n"
						+        "荷送人ＣＤ         = '" + sData[15] +"',"
						+        "荷送人部署名       = '" + sData[37] +"',"
						+        "集約店ＣＤ         = '" + s集約店ＣＤ +"', \n"
						+        "発店ＣＤ           = '" + s発店ＣＤ +"',"
						+        "発店名             = '" + s発店名   +"',"
						+        "得意先ＣＤ         = '" + sData[16] +"', \n"
						+        "部課ＣＤ           = '" + sData[17] +"',"
						+        "部課名             = '" + sData[18] +"',"
						+        "個数               =  " + sData[19] +", \n"
						+        "才数               =  " + sData[38] +","
						+        "重量               =  " + sData[20] +","
						+        "指定日             = '" + sData[21] +"',"
						+        "指定日区分         = '" + sData[41] +"',"
						+        "輸送指示ＣＤ１     = '" + sData[39] +"',"
						+        "輸送指示１         = '" + sData[22] +"', \n"
						+        "輸送指示ＣＤ２     = '" + sData[40] +"',"
						+        "輸送指示２         = '" + sData[23] +"',"
						+        "品名記事１         = '" + sData[24] +"',"
						+        "品名記事２         = '" + sData[25] +"', \n"
						+        "品名記事３         = '" + sData[26] +"',"
						// MOD 2011.07.14 東都）高木 記事行の追加 START
						+        "品名記事４         = '" + s品名記事４ +"', \n"
						+        "品名記事５         = '" + s品名記事５ +"',"
						+        "品名記事６         = '" + s品名記事６ +"', \n"
						// MOD 2011.07.14 東都）高木 記事行の追加 END
						+        "保険金額           =  " + sData[28] +","
						+        "仕分ＣＤ           = '" + s仕分ＣＤ + "', \n"
						+        "送り状発行済ＦＧ   = '0', \n"
						+        "送信済ＦＧ         = '0',"
						+        "状態               = '01',"
						+        "詳細状態           = '  ', \n"
						+        "更新ＰＧ           = '" + sData[32] +"',"
						+        "更新者             = '" + sData[33] +"', \n"
						+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE 会員ＣＤ           = '" + sData[0]  +"' \n"
						+ "   AND 部門ＣＤ           = '" + sData[1]  +"' \n"
						+ "   AND 登録日             = '" + sData[35] +"' \n"
						+ "   AND \"ジャーナルＮＯ\" = '" + sData[34] +"' \n"
						+ "   AND 更新日時           =  " + sData[36] +"";
					logWriter(sUser, INF, "出荷更新["+sData[1]+"]["+sData[35]+"]["+sData[34]+"]:["+sNo+"]");

					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					tran.Commit();
					if(iUpdRow == 0)
						sRet[0] = "データ編集中に他の端末より更新されています。\r\n再度、最新データを呼び出して更新してください。";
					else
						sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 出荷データ登録
		 * 引数：会員ＣＤ、部門ＣＤ、出荷日...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(736):
		*/
		[WebMethod]
		public String[] Ins_syukka(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "出荷登録開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			decimal d件数;
			string s特殊計 = " ";
			string s登録日;
			int i管理ＮＯ;
			string s日付;
			try
			{
				//出荷日チェック
				string[] sSyukkabi = Get_bumonsyukka(sUser, conn2, sData[0], sData[1]);
				sRet[0] = sSyukkabi[0];
				if(sRet[0] != " ") return sRet;
				if(int.Parse(sData[2]) < int.Parse(sSyukkabi[1]))
				{
					sRet[0] = "1";
					sRet[1] = sSyukkabi[1];
					return sRet;
				}

				//荷送人ＣＤ存在チェック
				string cmdQuery
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					//					= "SELECT 得意先ＣＤ, 得意先部課ＣＤ \n"
					//					+ "  FROM ＳＭ０１荷送人 \n"
					//					+ " WHERE 会員ＣＤ   = '" + sData[0]  +"' \n"
					//					+ "   AND 部門ＣＤ   = '" + sData[1]  +"' \n"
					//					+ "   AND 荷送人ＣＤ = '" + sData[15] +"' \n"
					//					+ "   AND 削除ＦＧ   = '0'";
					= "SELECT SM01.得意先ＣＤ, SM01.得意先部課ＣＤ \n"
					+ "     , NVL(CM01.保留印刷ＦＧ,'0') \n"
					+ "  FROM ＳＭ０１荷送人 SM01 \n"
					+ "     , ＣＭ０１会員 CM01 \n"
					+ " WHERE SM01.会員ＣＤ   = '" + sData[0]  +"' \n"
					+ "   AND SM01.部門ＣＤ   = '" + sData[1]  +"' \n"
					+ "   AND SM01.荷送人ＣＤ = '" + sData[15] +"' \n"
					+ "   AND SM01.削除ＦＧ   = '0' \n"
					+ "   AND SM01.会員ＣＤ   = CM01.会員ＣＤ(+) \n"
					;
				string s重量入力制御 = "0";
				// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if(reader.Read())
				{
					d件数 = 1;
					sData[16] = reader.GetString(0);
					sData[17] = reader.GetString(1);
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					s重量入力制御 = reader.GetString(2);
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				}
				else
				{
					d件数 = 0;
				}
				disposeReader(reader);
				reader = null;
				if(d件数 == 0)
				{
					sRet[0] = "0";
				}
				else
				{
					cmdQuery
						= "SELECT SM04.得意先部課名 \n"
						+ " FROM ＣＭ０２部門 CM02 \n"
						+    " , ＳＭ０４請求先 SM04 \n"
						+ " WHERE CM02.会員ＣＤ = '" + sData[0] + "' \n"
						+  " AND CM02.部門ＣＤ = '" + sData[1] + "' \n"
						+  " AND CM02.削除ＦＧ = '0' \n"
						+  " AND SM04.郵便番号 = CM02.郵便番号 \n"
						+  " AND SM04.得意先ＣＤ = '" + sData[16] + "' \n"
						+  " AND SM04.得意先部課ＣＤ = '" + sData[17] + "' \n"
						// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
						+  " AND SM04.会員ＣＤ = CM02.会員ＣＤ \n"
						// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
						+  " AND SM04.削除ＦＧ = '0' \n"
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

					//特殊計取得
					if(sData[4] != " ")
					{
						cmdQuery
							= "SELECT NVL(特殊計,' ') \n"
							+ "  FROM ＳＭ０２荷受人 \n"
							+ " WHERE 会員ＣＤ   = '" + sData[0] +"' \n"
							+ "   AND 部門ＣＤ   = '" + sData[1] +"' \n"
							+ "   AND 荷受人ＣＤ = '" + sData[4] +"' \n"
							+ "   AND 削除ＦＧ   = '0'";

						reader = CmdSelect(sUser, conn2, cmdQuery);
						bool bRead = reader.Read();
						if(bRead == true)
							s特殊計   = reader.GetString(0);

						disposeReader(reader);
						reader = null;
						cmdQuery
							= "UPDATE ＳＭ０２荷受人 \n"
							+ " SET 登録ＰＧ = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ " WHERE 会員ＣＤ = '" + sData[0] +"' \n"
							+ " AND 部門ＣＤ   = '" + sData[1] +"' \n"
							+ " AND 荷受人ＣＤ = '" + sData[4] +"' \n"
							+ " AND 削除ＦＧ   = '0'";
						try
						{
							int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
						}
						catch(Exception)
						{
							;
						}
					}

					//ジャーナルＮＯ取得
					cmdQuery
						= "SELECT \"ジャーナルＮＯ登録日\",\"ジャーナルＮＯ管理\", \n"
						+ "       TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+ "  FROM ＣＭ０２部門 \n"
						+ " WHERE 会員ＣＤ = '" + sData[0] +"' \n"
						+ "   AND 部門ＣＤ = '" + sData[1] +"' \n"
						+ "   AND 削除ＦＧ = '0'"
						+ "   FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					reader.Read();
					s登録日   = reader.GetString(0).Trim();
					i管理ＮＯ = reader.GetInt32(1);
					s日付     = reader.GetString(2);

					if(s登録日 == s日付)
						i管理ＮＯ++;
					else
					{
						s登録日 = s日付;
						i管理ＮＯ = 1;
					}

					cmdQuery 
						= "UPDATE ＣＭ０２部門 \n"
						+    "SET \"ジャーナルＮＯ登録日\"  = '" + s登録日 +"', \n"
						+        "\"ジャーナルＮＯ管理\"    = " + i管理ＮＯ +", \n"
						+        "更新ＰＧ                  = '" + sData[32] +"', \n"
						+        "更新者                    = '" + sData[33] +"', \n"
						+        "更新日時                  =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE 会員ＣＤ       = '" + sData[0] +"' \n"
						+ "   AND 部門ＣＤ       = '" + sData[1] +"' \n"
						+ "   AND 削除ＦＧ = '0'";

					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					disposeReader(reader);
					reader = null;

					//着店取得
					string[] sTyakuten = Get_tyakuten3(sUser, conn2, 
						sData[0], sData[1], sData[4], 
						sData[13] + sData[14], sData[8] + sData[9] + sData[10], sData[11] + sData[12]);
					sRet[0] = sTyakuten[0];
					if(sRet[0] != " ") return sRet;
					string s着店ＣＤ = (sTyakuten[1].Length > 0) ? sTyakuten[1] : " ";
					string s着店名   = (sTyakuten[2].Length > 0) ? sTyakuten[2] : " ";
					string s住所ＣＤ = (sTyakuten[3].Length > 0) ? sTyakuten[3] : " ";

					//発店取得
					string[] sHatuten = Get_hatuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sHatuten[0];
					if(sRet[0] != " ") return sRet;
					string s発店ＣＤ = sHatuten[1];
					string s発店名   = sHatuten[2];

					//集荷店取得
					string[] sSyuyaku = Get_syuuyakuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sSyuyaku[0];
					if(sRet[0] != " ") return sRet;
					string s集約店ＣＤ = sSyuyaku[1];

					//仕分ＣＤ取得
					string s仕分ＣＤ = " ";
					if(s発店ＣＤ.Trim().Length > 0 && s着店ＣＤ.Trim().Length > 0)
					{
						string[] sRetSiwake = Get_siwake(sUser, conn2, s発店ＣＤ, s着店ＣＤ);
						s仕分ＣＤ = sRetSiwake[1];
					}

					// MOD 2011.04.13 東都）高木 重量入力不可対応 START
					// MOD 2011.07.14 東都）高木 記事行の追加 START
					//					// 処理０２に才数および重量の参考値を入れる
					//					string s才数 = "";
					//					string s重量 = "";
					//					string s才数重量 = "";
					//					try{
					//						s才数 = sData[38].Trim().PadLeft(5,'0');
					//						s重量 = sData[20].Trim().PadLeft(5,'0');
					//						s才数重量 = s才数.Substring(0,5)
					//									+ s重量.Substring(0,5);
					//					}catch(Exception){
					//					}
					// MOD 2011.07.14 東都）高木 記事行の追加 END
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					///					string s重量入力制御 = (sData.Length > 42) ? sData[42] : "0";
					///					if(s重量入力制御 != "1"){
					///					string s重量入力制御 = (sData.Length > 42) ? sData[42] : " ";
					if(s重量入力制御 == "0")
					{
						// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						sData[38] = "0"; // 才数
						sData[20] = "0"; // 重量
						// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					}
					// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					// MOD 2011.04.13 東都）高木 重量入力不可対応 END
					// MOD 2011.07.14 東都）高木 記事行の追加 START
					string s品名記事４ = (sData.Length > 43) ? sData[43] : " ";
					string s品名記事５ = (sData.Length > 44) ? sData[44] : " ";
					string s品名記事６ = (sData.Length > 45) ? sData[45] : " ";
					if(s品名記事４.Length == 0) s品名記事４ = " ";
					// MOD 2011.07.14 東都）高木 記事行の追加 END
					cmdQuery 
						= "INSERT INTO \"ＳＴ０１出荷ジャーナル\" \n"
						+ "(会員ＣＤ, 部門ＣＤ, 登録日, \"ジャーナルＮＯ\", 出荷日 \n"
						+ ", お客様出荷番号, 荷受人ＣＤ \n"
						+ ", 電話番号１, 電話番号２, 電話番号３, ＦＡＸ番号１, ＦＡＸ番号２, ＦＡＸ番号３ \n"
						+ ", 住所ＣＤ, 住所１, 住所２, 住所３ \n"
						+ ", 名前１, 名前２, 名前３ \n"
						+ ", 郵便番号 \n"
						+ ", 着店ＣＤ, 着店名, 特殊計 \n"
						+ ", 荷送人ＣＤ, 荷送人部署名 \n"
						+ ", 集約店ＣＤ, 発店ＣＤ, 発店名 \n"
						+ ", 得意先ＣＤ, 部課ＣＤ, 部課名 \n"
						+ ", 個数, 才数, 重量, ユニット \n"
						+ ", 指定日, 指定日区分 \n"
						+ ", 輸送指示ＣＤ１, 輸送指示１ \n"
						+ ", 輸送指示ＣＤ２, 輸送指示２ \n"
						+ ", 品名記事１, 品名記事２, 品名記事３ \n"
						// MOD 2011.07.14 東都）高木 記事行の追加 START
						+ ", 品名記事４, 品名記事５, 品名記事６ \n"
						// MOD 2011.07.14 東都）高木 記事行の追加 END
						+ ", 元着区分, 保険金額, 運賃, 中継, 諸料金 \n"
						+ ", 仕分ＣＤ, 送り状番号, 送り状区分 \n"
						+ ", 送り状発行済ＦＧ, 出荷済ＦＧ, 送信済ＦＧ, 一括出荷ＦＧ \n"
						+ ", 状態, 詳細状態 \n"
						// MOD 2011.04.13 東都）高木 重量入力不可対応 START
						+ ", 処理０２ \n"
						// MOD 2011.04.13 東都）高木 重量入力不可対応 END
						+ ", 削除ＦＧ, 登録日時, 登録ＰＧ, 登録者 \n"
						+ ", 更新日時, 更新ＰＧ, 更新者 \n"
						+ ") \n"
						+ "VALUES ('" + sData[0] +"','" + sData[1] +"','" + s日付 +"'," + i管理ＮＯ +",'" + sData[2] +"', \n"
						+         "'" + sData[3] +"','" + sData[4] +"', \n"
						+         "'" + sData[5] +"','" + sData[6] +"','" + sData[7] +"',' ',' ',' ', \n"
						+         "'" + s住所ＣＤ +"','" + sData[8] +"','" + sData[9] +"','" + sData[10] +"', \n"
						+         "'" + sData[11] +"','" + sData[12] +"',' ', \n"
						+         "'" + sData[13] + sData[14] +"', \n"
						+         "'" + s着店ＣＤ +"','" + s着店名 + "','" + s特殊計 +"', \n"        //着店ＣＤ　着店名　特殊計
						+         "'" + sData[15] +"','" + sData[37] +"', \n"						  // 荷送人ＣＤ  荷送人部署名
						+         "'" + s集約店ＣＤ + "','" + s発店ＣＤ + "','" + s発店名 + "', \n"  //集約店ＣＤ　発店ＣＤ　発店名
						+         "'" + sData[16] +"','" + sData[17] +"','" + sData[18] +"', \n"
						+         "" + sData[19] +"," + sData[38] +"," + sData[20] +",0, \n"
						+         "'" + sData[21] +"','" + sData[41] +"', \n"
						+         "'" + sData[39] +"','" + sData[22] +"', \n"
						+         "'" + sData[40] +"','" + sData[23] +"', \n"
						+         "'" + sData[24] +"','" + sData[25] +"','" + sData[26] +"', \n"
						// MOD 2011.07.14 東都）高木 記事行の追加 START
						+         "'" + s品名記事４ +"','"+ s品名記事５ +"','"+ s品名記事６ +"', \n"
						// MOD 2011.07.14 東都）高木 記事行の追加 END
						+         "'" + sData[27] +"'," + sData[28] +",0,0,0, \n"  //運賃　中継　諸料金
						+         "'" + s仕分ＣＤ + "',' ',' ',"  //  仕分ＣＤ  送り状番号  送り状区分
						+         "'" + sData[29] +"','" + sData[30] +"', '0', '" + sData[31] +"', \n"  //   送信済ＦＧ
						+         "'01','  ', \n"        //状態　詳細状態
						// MOD 2011.04.13 東都）高木 重量入力不可対応 START
						// MOD 2011.07.14 東都）高木 記事行の追加 START
						//						+         "'" + s才数重量 + "', \n" // 処理０２
						+         "' ', \n" // 処理０２
						// MOD 2011.07.14 東都）高木 記事行の追加 END
						// MOD 2011.04.13 東都）高木 重量入力不可対応 END
						+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[32] +"','" + sData[33] +"', \n"
						+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[32] +"','" + sData[33] +"')";
					logWriter(sUser, INF, "出荷登録["+sData[1]+"]["+s日付+"]["+i管理ＮＯ+"]");

					iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					tran.Commit();
					sRet[0] = "正常終了";
					sRet[1] = s日付;
					sRet[2] = i管理ＮＯ.ToString();
				}

			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
				if(ex.Number == 1438)
				{ // ORA-01438: value larger than specified precision allows for this column
					//					if(i管理ＮＯ > 9999){
					sRet[0] = "１日で扱える出荷数（9999件）を越えました。";
					//					}
				}
			}
			catch (Exception ex)
			{
				tran.Rollback();
				string sErr = ex.Message.Substring(0,9);
				if(sErr == "ORA-00001")
					sRet[0] = "同一のコードが既に他の端末より登録されています。\r\n再度、最新データを呼び出して更新してください。";
				else
					sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 部門マスタ出荷日取得
		 * 引数：会員ＣＤ、部門ＣＤ
		 * 戻値：ステータス、出荷日
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(2246):
		*/
		private String[] Get_bumonsyukka(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[2];

			string cmdQuery = "SELECT 出荷日 \n"
				+ " FROM ＣＭ０２部門 \n"
				+ " WHERE 会員ＣＤ   = '" + sKcode + "' \n"
				+ "   AND 部門ＣＤ   = '" + sBcode + "' \n"
				+ "   AND 削除ＦＧ   = '0' \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0).Trim();
			}
			else
			{
				sRet[0] = "出荷日エラー";
				sRet[1] = "0";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
	
		}

		/*********************************************************************
		 * 仕分ＣＤ取得
		 * 引数：会員ＣＤ、部門ＣＤ、ＤＢ接続、発店、着店
		 * 戻値：ステータス、仕分ＣＤ
		 *
		 *********************************************************************/
		private static string GET_SIWAKE_SELECT
			= "SELECT 仕分ＣＤ \n"
			+ " FROM ＣＭ１７仕分 \n"
			;

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(2206):
		*/
		private String[] Get_siwake(string[] sUser, OracleConnection conn2, string sHatuCd, string sTyakuCd)
		{
			//			logWriter(sUser, INF, "仕分ＣＤ取得開始");

			string[] sRet = new string[2];

			string cmdQuery = GET_SIWAKE_SELECT
				+ " WHERE 発店所ＣＤ = '" + sHatuCd + "' \n"
				+ " AND 着店所ＣＤ = '" + sTyakuCd + "' \n"
				+ " AND 削除ＦＧ = '0' \n"
				;

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			if(reader.Read())
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0);
			}
			else
			{
				sRet[0] = "仕分ＣＤを決められませんでした";
				sRet[1] = " ";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * 住所マスタ一覧取得
		 * 引数：郵便番号
		 * 戻値：ステータス、一覧（郵便番号、都道府県名）...
		 *
		 * 参照元：住所検索.cs
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(3993):
		*/
		[WebMethod]
		public String[] Get_byPostcodeM(string[] sUser, string s郵便番号)
		{
			logWriter(sUser, INF, "住所マスタ一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(CM13.郵便番号) || '|' "
					+ "|| TRIM(CM13.都道府県名) || TRIM(CM13.市区町村名) || TRIM(CM13.大字通称名) || '|' "			//住所
					+ "|| TRIM(CM13.都道府県ＣＤ) || TRIM(CM13.市区町村ＣＤ) || TRIM(CM13.大字通称ＣＤ) || '|' "	//住所ＣＤ
					+ "|| NVL(CM10.店所名, ' ') || '|' \n"
					+  " FROM ＣＭ１３住所Ｊ CM13 \n" // 王子運送対応
					+  " LEFT JOIN ＣＭ１０店所 CM10 \n"
					+    " ON CM13.店所ＣＤ = CM10.店所ＣＤ "
					+    "AND CM10.削除ＦＧ = '0' \n";
				if(s郵便番号.Length == 7)
				{
					cmdQuery += " WHERE CM13.郵便番号 = '" + s郵便番号 + "' \n";
				}
				else
				{
					cmdQuery +=  " WHERE CM13.郵便番号 LIKE '" + s郵便番号 + "%' \n";
				}
				cmdQuery +=    " AND CM13.削除ＦＧ = '0' \n"
					+  " ORDER BY 大字通称ＣＤ \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0).Trim());
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 住所マスタ一覧取得(市)
		 * 引数：都道府県ＣＤ、市区町村ＣＤ
		 * 戻値：ステータス、一覧（郵便番号、大字通称名）...
		 *
		 *********************************************************************/
		private static string GET_BYKENSHIM_SELECT
			= "SELECT '|' || TRIM(MAX(CM13.郵便番号)) || '|' "
			+ "|| TRIM(MAX(CM13.大字通称名)) || '|' "
			+ "|| TRIM(MAX(CM13.都道府県ＣＤ))"
			+ "|| TRIM(MAX(CM13.市区町村ＣＤ))"
			+ "|| TRIM(MAX(CM13.大字通称ＣＤ)) || '|' "
			+ "|| MIN(NVL(CM10.店所名, ' ')) || '|' \n"
			+  " FROM ＣＭ１３住所Ｊ CM13 \n" // 王子運送対応
			+  " LEFT JOIN ＣＭ１０店所 CM10 \n"
			+    " ON CM13.店所ＣＤ = CM10.店所ＣＤ "
			+    "AND CM10.削除ＦＧ = '0' \n"
			;
		private static string GET_BYKENSHIM_WHERE
			= " AND CM13.削除ＦＧ = '0' \n"
			+ " GROUP BY CM13.大字通称ＣＤ \n"
			+ " ORDER BY CM13.大字通称ＣＤ \n"
			;
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(3884):
		*/
		[WebMethod]
		public String[] Get_byKenShiM(string[] sUser, string s都道府県ＣＤ, string s市区町村ＣＤ)
		{
			logWriter(sUser, INF, "住所マスタ一覧取得(市)開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= GET_BYKENSHIM_SELECT
					+ " WHERE CM13.都道府県ＣＤ = '" + s都道府県ＣＤ + "' \n"
					+   " AND CM13.市区町村ＣＤ = '" + s市区町村ＣＤ + "' \n"
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
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 大字通称名一覧の取得
		 * 引数：都道府県ＣＤ、市区町村ＣＤ
		 * 戻値：ステータス、大字通称名一覧
		 *
		 *********************************************************************/
		private static string GET_BYKENSHI_SELECT
			= "SELECT MAX(郵便番号), 大字通称名, 大字通称カナ名, MAX(都道府県ＣＤ), MAX(市区町村ＣＤ), 大字通称ＣＤ, MAX(店所ＣＤ) \n"
			+   "FROM ＣＭ１３住所Ｊ \n"; // 王子運送対応

		private static string GET_BYKENSHI_ORDER
			=    "AND 削除ＦＧ = '0' \n"
			+  "GROUP BY 大字通称ＣＤ,大字通称名,大字通称カナ名 \n"
			+  "ORDER BY 大字通称カナ名, 大字通称ＣＤ \n"
			;

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2address\Service1.asmx.cs(286):
		*/
		[WebMethod]
		public String[] Get_byKenShi(string[] sUser, string s都道府県ＣＤ, string s市区町村ＣＤ)
		{
			logWriter(sUser, INF, "大字通称名一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKENSHI_SELECT);
				sbQuery.Append(" WHERE 都道府県ＣＤ = '" + s都道府県ＣＤ + "' \n");
				sbQuery.Append("   AND 市区町村ＣＤ = '" + s市区町村ＣＤ + "' \n");
				sbQuery.Append(GET_BYKENSHI_ORDER);
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));
					sbRet.Append("|" + reader.GetString(1).Trim());
					sbRet.Append("|D" + "|");
					sbRet.Append(reader.GetString(2).Trim());		// 大字通称カナ名
					sbRet.Append("|" + reader.GetString(3).Trim());	// 都道府県ＣＤ
					sbRet.Append(reader.GetString(4).Trim());		// 市区町村ＣＤ
					sbRet.Append(reader.GetString(5).Trim());		// 大字通称ＣＤ
					sbRet.Append("|" + reader.GetString(6).Trim());	// 店所ＣＤ

					sList.Add(sbRet);
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 住所一覧の取得
		 * 引数：郵便番号
		 * 戻値：ステータス、住所一覧
		 *
		 *********************************************************************/
		private static string GET_BYPOSTCODE_SELECT
			= "SELECT 郵便番号, 都道府県名, 市区町村名, 大字通称名, 大字通称カナ名, 都道府県ＣＤ, 市区町村ＣＤ, 大字通称ＣＤ, 店所ＣＤ \n"
			+  " FROM ＣＭ１３住所Ｊ \n"; // 王子運送対応

		private static string GET_BYPOSTCODE_ORDER
			=    "AND 削除ＦＧ = '0' \n"
			+  "ORDER BY 郵便番号, 大字通称カナ名 \n";

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2address\Service1.asmx.cs(415):
		*/
		[WebMethod]
		public String[] Get_byPostcode(string[] sUser, string s郵便番号)
		{
			logWriter(sUser, INF, "住所一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYPOSTCODE_SELECT);
				if(s郵便番号.Length == 7)
				{
					sbQuery.Append(" WHERE 郵便番号 = '" + s郵便番号 + "' ");
				}
				else
				{
					sbQuery.Append(" WHERE 郵便番号 LIKE '" + s郵便番号 + "%' ");
				}
				sbQuery.Append(GET_BYPOSTCODE_ORDER);

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));		// 郵便番号
					sbRet.Append("|" + reader.GetString(1).Trim());	// 都道府県名
					sbRet.Append(reader.GetString(2).Trim());		// 市区町村名
					sbRet.Append(reader.GetString(3).Trim());		// 大字通称名
					sbRet.Append("|D" + "|");
					sbRet.Append(reader.GetString(4).Trim());		// 大字通称カナ名
					sbRet.Append("|" + reader.GetString(5).Trim());	// 都道府県ＣＤ
					sbRet.Append(reader.GetString(6).Trim());		// 市区町村ＣＤ
					sbRet.Append(reader.GetString(7).Trim());		// 大字通称ＣＤ
					sbRet.Append("|" + reader.GetString(8).Trim());	// 店所ＣＤ
					sList.Add(sbRet);

				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 申込情報追加
		 * 引数：管理番号、会員名...
		 * 戻値：ステータス、更新日時、管理番号
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(5972):
		*/
		[WebMethod]
		public string[] Ins_Mosikomi(string[] sUser, string[] sData)
		{
			//管理番号の取得
			string[] sKey   = {" ", sData[42]};	//店所ＣＤ、更新者
			string[] sKanri = Get_KaniSaiban(sUser, sKey);
			if(sKanri[0].Length > 4)
			{
				return sKanri;
			}
			sData[0] = sKanri[1];

			logWriter(sUser, INF, "申込情報追加開始");

			OracleConnection conn2 = null;

			string s更新日時 = System.DateTime.Now.ToString("yyyyMMddHHmmss");
			string[] sRet = new string[3]{"", s更新日時, sData[0]};

			string s更新ＰＧ = "申込登録";
			if(sData.Length > 43)
				s更新ＰＧ = sData[43];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT 削除ＦＧ \n"
					+   "FROM ＳＭ０５会員申込 \n"
					+  "WHERE 管理番号 = " + sData[0] + " \n"
					+    "FOR UPDATE \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				string s削除ＦＧ = "";
				if(reader.Read())
				{
					s削除ＦＧ = reader.GetString(0);
					iCnt++;
				}

				if(iCnt == 1)
				{
					//追加
					cmdQuery
						= "INSERT INTO ＳＭ０５会員申込 \n"
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
						+         "," + s更新日時
						+         ",'" + s更新ＰＧ + "' "
						+         ",'" + sData[42] + "' \n"
						+         "," + s更新日時
						+         ",'" + s更新ＰＧ + "' "
						+         ",'" + sData[42] + "' \n"
						+ " ) \n";

					CmdUpdate(sUser, conn2, cmdQuery);

					tran.Commit();
					sRet[0] = "正常終了";


				}
				else
				{
					//追加更新
					if (s削除ＦＧ.Equals("1"))
					{
						cmdQuery
							= "UPDATE ＳＭ０５会員申込 \n"
							+   " SET 店所ＣＤ = '" + sData[1] + "' \n"
							+       ",申込者カナ = '" + sData[2] + "' \n"
							+       ",申込者名 = '" + sData[3] + "' \n"
							+       ",申込者郵便番号 = '" + sData[4] + "' \n"
							+       ",申込者県ＣＤ = '" + sData[5] + "' \n"
							+       ",申込者住所１ = '" + sData[6] + "' \n"
							+       ",申込者住所２ = '" + sData[7] + "' \n"
							+       ",申込者電話１ = '" + sData[8] + "' \n"
							+       ",申込者電話２ = '" + sData[9] + "' \n"
							+       ",申込者電話３ = '" + sData[10] + "' \n"
							+       ",申込者電話 = '" + sData[11] + "' \n"
							+       ",申込者ＦＡＸ１ = '" + sData[12] + "' \n"
							+       ",申込者ＦＡＸ２ = '" + sData[13] + "' \n"
							+       ",申込者ＦＡＸ３ = '" + sData[14] + "' \n"
							+       ",設置場所区分 = '" + sData[15] + "' \n"
							+       ",設置場所カナ = '" + sData[16] + "' \n"
							+       ",設置場所名 = '" + sData[17] + "' \n"
							+       ",設置場所郵便番号 = '" + sData[18] + "' \n"
							+       ",設置場所県ＣＤ = '" + sData[19] + "' \n"
							+       ",設置場所住所１ = '" + sData[20] + "' \n"
							+       ",設置場所住所２ = '" + sData[21] + "' \n"
							+       ",設置場所電話１ = '" + sData[22] + "' \n"
							+       ",設置場所電話２ = '" + sData[23] + "' \n"
							+       ",設置場所電話３ = '" + sData[24] + "' \n"
							+       ",設置場所ＦＡＸ１ = '" + sData[25] + "' \n"
							+       ",設置場所ＦＡＸ２ = '" + sData[26] + "' \n"
							+       ",設置場所ＦＡＸ３ = '" + sData[27] + "' \n"
							+       ",設置場所担当者名 = '" + sData[28] + "' \n"
							+       ",設置場所役職名 = '" + sData[29] + "' \n"
							+       ",設置場所使用料 =  " + sData[30] + "  \n"
							+       ",会員ＣＤ = '" + sData[31] + "' \n"
							+       ",使用開始日 = '" + sData[32] + "' \n"
							+       ",部門ＣＤ = '" + sData[33] + "' \n"
							+       ",部門名 = '" + sData[34] + "' \n"
							+       ",サーマル台数 =  " + sData[35] + "  \n"
							+       ",利用者ＣＤ = '" + sData[36] + "' \n"
							+       ",利用者名 = '" + sData[37] + "' \n"
							+       ",パスワード = '" + sData[38] + "' \n"
							+       ",承認状態ＦＧ = '" + sData[39] + "' \n"
							+       ",メモ = '" + sData[40] + "' \n"
							+       ",削除ＦＧ = '0' \n"
							+       ",登録日時 = " + s更新日時 + " \n"
							+       ",登録ＰＧ = '" + s更新ＰＧ + "' \n"
							+       ",登録者 = '" + sData[42] + "' \n"
							+       ",更新日時 = " + s更新日時 + " \n"
							+       ",更新ＰＧ = '" + s更新ＰＧ + "' \n"
							+       ",更新者 = '" + sData[42] + "' \n"
							+ " WHERE 管理番号 = '" + sData[0] + "' \n";

						CmdUpdate(sUser, conn2, cmdQuery);

						string sRet会員   = "";
						string sRet部門   = "";
						string sRet利用者 = "";
						//承認状態ＦＧが[3：承認済]の場合
						if(sData[39].Equals("3"))
						{
							sRet会員 = Ins_Member2(sUser, conn2, sData, s更新日時);
							if(sRet会員.Length == 4)
							{
								//部門マスタ追加
								sRet部門 = Ins_Section2(sUser, conn2, sData, s更新日時);
								if(sRet部門.Length == 4)
								{
									//利用者マスタ追加
									sRet利用者 = Ins_User2(sUser, conn2, sData, s更新日時);
								}
							}
						}
						if(sRet会員.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "お客様：" + sRet会員;
						}
						else if(sRet部門.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "セクション：" + sRet部門;
						}
						else if(sRet利用者.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "ユーザー：" + sRet利用者;
						}
						else
						{
							tran.Commit();
							sRet[0] = "正常終了";

						}
					}
					else
					{
						tran.Rollback();
						sRet[0] = "既に登録されています";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 会員マスタ追加２
		 * 引数：会員ＣＤ、会員名...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(6792):
		*/
		private string Ins_Member2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			//会員マスタ追加
			string[] sKey = new string[4]{
											 sData[31],	//会員ＣＤ
											 sData[3],	//申込者名
											 sData[32],	//使用開始日
											 sData[42]	//登録者、更新者
										 };

			string sRet = "";

			string cmdQuery = "";
			cmdQuery
				= "SELECT 削除ＦＧ \n"
				+   "FROM ＣＭ０１会員 \n"
				+  "WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string s削除ＦＧ = "";
			while (reader.Read())
			{
				s削除ＦＧ = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//追加
				cmdQuery
					= "INSERT INTO ＣＭ０１会員 \n"
					+ " VALUES ('" + sKey[0] + "' "		//会員ＣＤ
					+         ",'" + sKey[1] + "' "		//会員名
					+         ",'" + sKey[2] + "' "		//使用開始日
					+         ",'99999999' "			//使用終了日
					+         ",'3' \n"					//管理者区分 // 3:王子一般
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
					+         ",'会員登録' "
					+         ",'" + sKey[3] + "' \n"
					+         "," + sUpdateTime
					+         ",'会員登録' "
					+         ",'" + sKey[3] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);

				sRet = "正常終了";
			}
			else
			{
				//追加更新
				if (s削除ＦＧ.Equals("1"))
				{
					cmdQuery
						= "UPDATE ＣＭ０１会員 \n"
						+   " SET 会員名 = '" + sKey[1] + "' \n"
						+       ",使用開始日 = '" + sKey[2] + "' \n"
						+       ",使用終了日 = '99999999' \n"
						+       ",管理者区分 = '3' \n" // 3:王子一般
						// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
						+       ",記事連携ＦＧ = '0' \n"
						+       ",保留印刷ＦＧ = '0' \n"
						// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						+       ",削除ＦＧ = '0' \n"
						+       ",登録日時 = " + sUpdateTime
						+       ",登録ＰＧ = '会員登録' "
						+       ",登録者 = '" + sKey[3] + "' \n"
						+       ",更新日時 = " + sUpdateTime
						+       ",更新ＰＧ = '会員登録' "
						+       ",更新者 = '" + sKey[3] + "' \n"
						+ " WHERE 会員ＣＤ = '" + sKey[0] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);

					sRet = "正常終了";
				}
				else
				{
					sRet = "既に登録されています";
				}
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * 管理番号の採番
		 * 引数：会員ＣＤ、部門ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(6655):
		*/
		[WebMethod]
		public String[] Get_KaniSaiban(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "管理番号の取得開始");
			
			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				decimal iカレント番号 = 0;
				decimal i開始番号     = 0;
				decimal i終了番号     = 0;

				string cmdQuery
					= "SELECT カレント番号, 開始番号, 終了番号 \n"
					+ " FROM ＣＭ１６店所採番管理 \n"
					+ " WHERE 採番区分 = '01' \n"
					+ " AND 店所ＣＤ = '" + sKey[0] + "' \n"
					+ " FOR UPDATE \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				string updQuery = "";
				if(reader.Read())
				{
					iカレント番号 = reader.GetDecimal(0);
					i開始番号     = reader.GetDecimal(1);
					i終了番号     = reader.GetDecimal(2);

					if(iカレント番号 < i終了番号)
					{
						iカレント番号++;
					}
					else
					{
						iカレント番号 = i開始番号;
					}
					sRet[1] = iカレント番号.ToString("0000000");

					updQuery 
						= "UPDATE ＣＭ１６店所採番管理 SET \n"
						+ "  カレント番号 = " + iカレント番号 + " \n"
						+ ", 開始番号 = " + i開始番号 + " \n"
						+ ", 終了番号 = " + i終了番号 + " \n"
						+ ", 更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ ", 更新ＰＧ = '会員申込' \n"
						+ ", 更新者 = '" + sKey[1] + "' \n"
						+ " WHERE 採番区分 = '01' \n"
						+ " AND 店所ＣＤ = '" + sKey[0] + "' \n"
						+ " AND 削除ＦＧ = '0' \n";
				}
				else
				{
					iカレント番号 = 5005001;
					i開始番号     = 1000001;
					i終了番号     = 9999999;
					sRet[1] = iカレント番号.ToString("0000000");

					// 送り状採番の追加
					updQuery 
						= "INSERT INTO ＣＭ１６店所採番管理 VALUES( \n"
						+ " '01' \n"
						+ ",'" + sKey[0] + "' \n"
						+ ", " + iカレント番号 + " \n"
						+ ", " + i開始番号 + " \n"
						+ ", " + i終了番号 + " \n"
						+ ",'0' \n"
						+ ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
						+ ",'会員申込' "
						+ ",'" + sKey[1] + "' \n"
						+ ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
						+ ",'会員申込' "
						+ ",'" + sKey[1] + "' \n"
						+ ") \n";
				}
				CmdUpdate(sUser, conn2, updQuery);
				disposeReader(reader);
				reader = null;
				tran.Commit();
				sRet[0] = "正常終了";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 部門マスタ追加２
		 * 引数：会員ＣＤ、部門ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(6904):
		*/
		private string Ins_Section2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			string[] sKey = new string[10]{
											  sData[31],	//会員ＣＤ
											  sData[33],	//部門ＣＤ
											  sData[34],	//部門名
											  sData[18],	//設置場所郵便番号
											  sData[20],	//設置場所住所１
											  sData[21],	//設置場所住所２
											  sData[35],	//サーマル台数
											  sData[42]	//登録者、更新者
											  ,sData[30]	//設置場所使用料
											  ,sData[0]	//管理番号
										  };
			string sRet = "";

			string cmdQuery = "";

			cmdQuery
				= "SELECT 削除ＦＧ \n"
				+   "FROM ＣＭ０２部門 \n"
				+  "WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
				+    "AND 部門ＣＤ = '" + sKey[1] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string s削除ＦＧ = "";
			while (reader.Read())
			{
				s削除ＦＧ = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//追加
				cmdQuery
					= "INSERT INTO ＣＭ０２部門 \n"
					+         "(会員ＣＤ \n"
					+         ",部門ＣＤ \n"
					+         ",部門名 \n"
					+         ",組織ＣＤ \n"
					+         ",出力順 \n"
					+         ",郵便番号 \n"
					+         ",\"ジャーナルＮＯ登録日\" \n"
					+         ",\"ジャーナルＮＯ管理\" \n"
					+         ",雛型ＮＯ \n"
					+         ",出荷日 \n"
					+         ",設置先住所１ \n"
					+         ",設置先住所２ \n"
					+         ",サーマル台数 \n"
					+         ",削除ＦＧ \n"
					+         ",登録日時 \n"
					+         ",登録ＰＧ \n"
					+         ",登録者 \n"
					+         ",更新日時 \n"
					+         ",更新ＰＧ \n"
					+         ",更新者 \n"
					+         ") \n"
					+ " VALUES ('" + sKey[0] + "' "				//会員ＣＤ
					+         ",'" + sKey[1] + "' "				//部門ＣＤ
					+         ",'" + sKey[2] + "' "				//部門名
					+         ",' ' "							//組織ＣＤ
					+         ", 0 \n"							//出力順
					+         ",'" + sKey[3] + "' "				//郵便番号
					+         ",TO_CHAR(SYSDATE,'YYYYMMDD') "	//ジャーナルＮＯ登録日
					+         ", 0 "							//ジャーナル管理ＮＯ
					+         ", 0 "							//雛型ＮＯ
					+         ",TO_CHAR(SYSDATE,'YYYYMMDD') \n"	//出荷日
					+         ",'" + sKey[4] + "' "				//設置先住所１
					+         ",'" + sKey[5] + "' "				//設置先住所２
					+         ", " + sKey[6] + " \n"			//サーマル台数
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'会員登録' "
					+         ",'" + sKey[7] + "' \n"
					+         "," + sUpdateTime
					+         ",'会員登録' "
					+         ",'" + sKey[7] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);

				cmdQuery
					= "INSERT INTO ＣＭ０６部門拡張 \n"
					+         "(会員ＣＤ \n"
					+         ",部門ＣＤ \n"
					+         ",使用料 \n"
					+         ",会員申込管理番号 \n"
					+         ",削除ＦＧ \n"
					+         ",登録日時 \n"
					+         ",登録ＰＧ \n"
					+         ",登録者 \n"
					+         ",更新日時 \n"
					+         ",更新ＰＧ \n"
					+         ",更新者 \n"
					+         ") \n"
					+ " VALUES ('" + sKey[0] + "' "				//会員ＣＤ
					+         ",'" + sKey[1] + "' "				//部門ＣＤ
					+         ", " + sKey[8] + " \n"			//使用料
					+         ", " + sKey[9] + " \n"			//会員申込管理番号
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'会員登録' "
					+         ",'" + sKey[7] + "' \n"
					+         "," + sUpdateTime
					+         ",'会員登録' "
					+         ",'" + sKey[7] + "' \n"
					+ " ) \n";
				CmdUpdate(sUser, conn2, cmdQuery);

				sRet = "正常終了";
			}
			else
			{
				//追加更新
				if (s削除ＦＧ.Equals("1"))
				{
					cmdQuery
						= "UPDATE ＣＭ０２部門 \n"
						+   " SET 部門名 = '" + sKey[2] + "' \n"
						+       ",組織ＣＤ = ' ' \n"
						+       ",出力順 = 0 \n"
						+       ",郵便番号 = '" + sKey[3] + "' \n"
						+       ",ジャーナルＮＯ登録日 = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+       ",ジャーナルＮＯ管理 = 0 \n"
						+       ",雛型ＮＯ = 0 \n"
						+       ",出荷日 = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+       ",設置先住所１ = '" + sKey[4] + "' \n"
						+       ",設置先住所２ = '" + sKey[5] + "' \n"
						+       ",サーマル台数 =  " + sKey[6] + " \n"
						+       ",削除ＦＧ = '0' \n"
						+       ",登録日時 = " + sUpdateTime
						+       ",登録ＰＧ = '会員登録' "
						+       ",登録者 = '" + sKey[7] + "' \n"
						+       ",更新日時 = " + sUpdateTime
						+       ",更新ＰＧ = '会員登録' "
						+       ",更新者 = '" + sKey[7] + "'\n"
						+ " WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
						+   " AND 部門ＣＤ = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					cmdQuery
						= "UPDATE ＣＭ０６部門拡張 SET \n"
						+       " 使用料 = " + sKey[8] + " \n"
						+       ",会員申込管理番号 = " + sKey[9] + " \n"
						+       ",削除ＦＧ = '0' \n"
						+       ",登録日時 = " + sUpdateTime
						+       ",登録ＰＧ = '会員登録' "
						+       ",登録者 = '" + sKey[7] + "' \n"
						+       ",更新日時 = " + sUpdateTime
						+       ",更新ＰＧ = '会員登録' "
						+       ",更新者 = '" + sKey[7] + "'\n"
						+ " WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
						+   " AND 部門ＣＤ = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "正常終了";
				}
				else
				{
					sRet = "既に登録されています";
				}
			}
			disposeReader(reader);
			reader = null;

			//エラー時は、終了
			if (!sRet.Equals("正常終了")) return sRet;

			logWriter(sUser, INF, "記事の初期データ登録開始");

			//記事の初期データの検索
			cmdQuery
				= "SELECT 記事ＣＤ \n"
				+      ", 記事 \n"
				+   "FROM ＳＭ０３記事 \n"
				+  "WHERE 会員ＣＤ = 'default' \n"
				+    "AND 部門ＣＤ = '0000' \n"
				+    "AND 削除ＦＧ = '0' \n";

			OracleDataReader readerDef = CmdSelect(sUser, conn2, cmdQuery);
			string s初期記事ＣＤ = "";
			string s初期記事     = "";
			while (readerDef.Read())
			{
				s初期記事ＣＤ = readerDef.GetString(0);
				s初期記事     = readerDef.GetString(1);

				//記事の検索
				cmdQuery
					= "SELECT 記事ＣＤ \n"
					+   "FROM ＳＭ０３記事 \n"
					+  "WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
					+    "AND 部門ＣＤ = '" + sKey[1] + "' \n"
					+    "AND 記事ＣＤ = '" + s初期記事ＣＤ + "' \n"
					+    "FOR UPDATE \n";

				OracleDataReader readerNote = CmdSelect(sUser, conn2, cmdQuery);
				if (readerNote.Read())
				{
					//既に記事がある場合は新規更新
					cmdQuery
						= "UPDATE ＳＭ０３記事 \n"
						+   " SET 記事 = '" + s初期記事 + "' \n"
						+       ",削除ＦＧ = '0' \n"
						+       ",登録日時 = " + sUpdateTime
						+       ",登録ＰＧ = '初期記事' \n"
						+       ",登録者 = '" + sKey[7] + "' \n"
						+       ",更新日時 = " + sUpdateTime
						+       ",更新ＰＧ = '初期記事' \n"
						+       ",更新者 = '" + sKey[7] + "' \n"
						+ " WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
						+   " AND 部門ＣＤ = '" + sKey[1] + "' \n"
						+   " AND 記事ＣＤ = '" + s初期記事ＣＤ + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "正常終了";
				}
				else
				{
					//新規追加
					cmdQuery
						= "INSERT INTO ＳＭ０３記事 \n"
						+ " VALUES ('" + sKey[0] + "' " 
						+         ",'" + sKey[1] + "' "
						+         ",'" + s初期記事ＣＤ + "' "
						+         ",'" + s初期記事 + "' \n"
						+         ",'0' \n"
						+         "," + sUpdateTime
						+         ",'初期記事' "
						+         ",'" + sKey[7] + "' \n"
						+         "," + sUpdateTime
						+         ",'初期記事' "
						+         ",'" + sKey[7] + "' \n"
						+ " ) \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "正常終了";
				}
				disposeReader(readerNote);
				readerNote = null;
			}
			disposeReader(readerDef);
			readerDef = null;

			return sRet;
		}

		/*********************************************************************
		 * 利用者マスタ追加２
		 * 引数：会員ＣＤ、利用者ＣＤ、利用者名
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(7197):
		*/
		private string Ins_User2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			string[] sKey = new string[6]{
											 sData[31],	//会員ＣＤ
											 sData[36],	//利用者ＣＤ
											 sData[38],	//パスワード
											 sData[37],	//利用者名
											 sData[33],	//部門ＣＤ
											 sData[42]	//登録者、更新者
										 };
			string sRet = "";

			string cmdQuery = "";

			cmdQuery
				= "SELECT 削除ＦＧ \n"
				+   "FROM ＣＭ０４利用者 \n"
				+  "WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
				+    "AND 利用者ＣＤ = '" + sKey[1] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string s削除ＦＧ = "";
			while (reader.Read())
			{
				s削除ＦＧ = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//追加
				cmdQuery
					= "INSERT INTO ＣＭ０４利用者 \n"
					+ " VALUES ('" + sKey[0] + "' "		//会員ＣＤ
					+         ",'" + sKey[1] + "' "		//利用者ＣＤ
					+         ",'" + sKey[2] + "' "		//パスワード
					+         ",'" + sKey[3] + "' "		//利用者名
					+         ",'" + sKey[4] + "' \n"	//部門ＣＤ
					+         ",' ' "					//荷送人ＣＤ
					+         ",0 "						//認証エラー回数
					+         ",' ' "					//権限１
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
					+         ",'会員登録' "
					+         ",'" + sKey[5] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);
				sRet = "正常終了";
			}
			else
			{
				//追加更新
				if (s削除ＦＧ.Equals("1"))
				{
					cmdQuery
						= "UPDATE ＣＭ０４利用者 \n"
						+   " SET パスワード = '" + sKey[2] + "' \n"
						+       ",利用者名 = '" + sKey[3] + "' \n"
						+       ",部門ＣＤ = '" + sKey[4] + "' \n"
						+       ",荷送人ＣＤ = ' ' \n"
						+       ",認証エラー回数 = 0 \n"
						+       ",権限１ = ' ' \n"
						+       ",削除ＦＧ = '0' \n"
						+       ",登録日時 = " + sUpdateTime
						+       ",登録ＰＧ = '"+ sUpdateTime.Substring(0,8) +"' "
						+       ",登録者 = '" + sKey[5] + "' \n"
						+       ",更新日時 = " + sUpdateTime
						+       ",更新ＰＧ = '会員登録' "
						+       ",更新者 = '" + sKey[5] + "' \n"
						+ " WHERE 会員ＣＤ = '" + sKey[0] + "' \n"
						+   " AND 利用者ＣＤ = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "正常終了";
				}
				else
				{
					sRet = "既に登録されています";
				}
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * 申込情報更新
		 * 引数：管理番号、会員名...
		 * 戻値：ステータス、更新日時
		 *
		 *********************************************************************/
		private static string UPD_MOSIKOMI_SELECT
			= "SELECT 管理番号 "
			+ ", 店所ＣＤ "
			+ ", 申込者カナ "
			+ ", 申込者名 "
			+ ", 申込者郵便番号 \n"
			+ ", 申込者県ＣＤ "
			+ ", 申込者住所１ "
			+ ", 申込者住所２ "
			+ ", 申込者電話１ "
			+ ", 申込者電話２ \n"
			+ ", 申込者電話３ "
			+ ", 申込者電話 "
			+ ", 申込者ＦＡＸ１ "
			+ ", 申込者ＦＡＸ２ "
			+ ", 申込者ＦＡＸ３ \n"
			+ ", 設置場所区分 "
			+ ", 設置場所カナ "
			+ ", 設置場所名 "
			+ ", 設置場所郵便番号 "
			+ ", 設置場所県ＣＤ \n"
			+ ", 設置場所住所１ "
			+ ", 設置場所住所２ "
			+ ", 設置場所電話１ "
			+ ", 設置場所電話２ "
			+ ", 設置場所電話３ \n"
			+ ", 設置場所ＦＡＸ１ "
			+ ", 設置場所ＦＡＸ２ "
			+ ", 設置場所ＦＡＸ３ "
			+ ", 設置場所担当者名 "
			+ ", 設置場所役職名 \n"
			+ ", 設置場所使用料 "
			+ ", 会員ＣＤ "
			+ ", 使用開始日 "
			+ ", 部門ＣＤ "
			+ ", 部門名 \n"
			+ ", \"サーマル台数\" "
			+ ", 利用者ＣＤ "
			+ ", 利用者名 "
			+ ", \"パスワード\" "
			+ ", 承認状態ＦＧ \n"
			+ ", メモ "
			+ ", TO_CHAR(更新日時) "
			+ ", 更新者 \n"
			+ "FROM ＳＭ０５会員申込 \n"
			+ "";

		private static string UPD_MOSIKOMI_DELETE
			= "UPDATE ＳＭ０５会員申込 \n"
			+ "SET 削除ＦＧ = '1' \n"
			+ "";

		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2maintenance\Service1.asmx.cs(6303):
		*/
		[WebMethod]
		public string[] Upd_Mosikomi(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "申込情報更新開始");

			OracleConnection conn2 = null;
			string s更新日時 = System.DateTime.Now.ToString("yyyyMMddHHmmss");
			string[] sRet = new string[3]{"", s更新日時, sData[0]};

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			string cmdQuery = "";

			try
			{
				bool bUpdState = false;

				//承認状態ＦＧが[1：申請中]の場合（印刷ボタンの時）
				if(sData[39].Equals("1"))
				{
					string[] sRefData = new string[43];
					cmdQuery = UPD_MOSIKOMI_SELECT
						+ " WHERE 管理番号 = '" + sData[0] + "' \n"
						+ " AND 削除ＦＧ = '0' \n"
						+ " AND 更新日時 = " + sData[41] + " \n"
						+ " FOR UPDATE \n";

					OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read())
					{
						tran.Rollback();
						sRet[0] = "他の端末で更新されています";
						logWriter(sUser, INF, sRet[0]);
						return sRet;
					}
					sRefData[0] = "";
					//管理番号はダミー
					sRefData[1] = reader.GetString(1).Trim();
					sRefData[2] = reader.GetString(2).Trim();
					sRefData[3] = reader.GetString(3).Trim();
					sRefData[4] = reader.GetString(4).Trim();
					sRefData[5] = reader.GetString(5).Trim();	//申込者県ＣＤ
					sRefData[6] = reader.GetString(6).Trim();
					sRefData[7] = reader.GetString(7).Trim();
					sRefData[8] = reader.GetString(8).Trim();
					sRefData[9] = reader.GetString(9).Trim();
					sRefData[10] = reader.GetString(10).Trim();	//申込者電話３
					sRefData[11] = reader.GetString(11).Trim();
					sRefData[12] = reader.GetString(12).Trim();
					sRefData[13] = reader.GetString(13).Trim();
					sRefData[14] = reader.GetString(14).Trim();
					sRefData[15] = reader.GetString(15).Trim();	//設置場所区分
					sRefData[16] = reader.GetString(16).Trim();
					sRefData[17] = reader.GetString(17).Trim();
					sRefData[18] = reader.GetString(18).Trim();
					sRefData[19] = reader.GetString(19).Trim();
					sRefData[20] = reader.GetString(20).Trim();	//設置場所住所１
					sRefData[21] = reader.GetString(21).Trim();
					sRefData[22] = reader.GetString(22).Trim();
					sRefData[23] = reader.GetString(23).Trim();
					sRefData[24] = reader.GetString(24).Trim();
					sRefData[25] = reader.GetString(25).Trim();	//設置場所ＦＡＸ１
					sRefData[26] = reader.GetString(26).Trim();
					sRefData[27] = reader.GetString(27).Trim();
					sRefData[28] = reader.GetString(28).Trim();
					sRefData[29] = reader.GetString(29).Trim();
					sRefData[30] = reader.GetDecimal(30).ToString().Trim();	//設置場所使用料
					sRefData[31] = reader.GetString(31).Trim();
					sRefData[32] = reader.GetString(32).Trim();
					sRefData[33] = reader.GetString(33).Trim();
					sRefData[34] = reader.GetString(34).Trim();
					sRefData[35] = reader.GetDecimal(35).ToString().Trim();	//サーマル台数
					sRefData[36] = reader.GetString(36).Trim();
					sRefData[37] = reader.GetString(37).Trim();
					sRefData[38] = reader.GetString(38).Trim();
					sRefData[39] = reader.GetString(39).Trim();
					sRefData[40] = reader.GetString(40).Trim();	//メモ
					sRefData[41] = reader.GetString(41).Trim();
					sRefData[42] = reader.GetString(42).Trim();

					//承認状態ＦＧ（_:登録中、1:申請中、2:留保中、3:承認済）が
					//（1:申請中もしくは2:留保中のもの）
					if(sRefData[39].Length > 0)
					{
						//データの更新状況をチェックする
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
							//データ削除
							cmdQuery = UPD_MOSIKOMI_DELETE
								+ ", 更新ＰＧ = '申込更新' \n"
								+ ", 更新者   = '" + sData[42] +"' \n"
								+ ", 更新日時 = "+ s更新日時 + " \n"
								+ " WHERE 管理番号 = '" + sData[0] + "' \n"
								+ " AND 削除ＦＧ = '0' \n"
								+ " AND 更新日時 = " + sData[41] + " \n";

							if (CmdUpdate(sUser, conn2, cmdQuery) == 0)
							{
								tran.Rollback();
								sRet[0] = "他の端末で更新されています";
							}
							else
							{
								tran.Commit();
								sRet[0] = "正常終了";
							}
							logWriter(sUser, INF, sRet[0]);
							//データが変更されている場合には、新しい受注ＮＯでデータを追加する
							//保留　トランザクション制御
							return Ins_Mosikomi(sUser, sData);
						}
					}
					disposeReader(reader);
					reader = null;
				}

				cmdQuery
					= "UPDATE ＳＭ０５会員申込 \n"
					+   " SET 店所ＣＤ = '" + sData[1] + "' \n"
					+       ",申込者カナ = '" + sData[2] + "' \n"
					+       ",申込者名 = '" + sData[3] + "' \n"
					+       ",申込者郵便番号 = '" + sData[4] + "' \n"
					+       ",申込者県ＣＤ = '" + sData[5] + "' \n"
					+       ",申込者住所１ = '" + sData[6] + "' \n"
					+       ",申込者住所２ = '" + sData[7] + "' \n"
					+       ",申込者電話１ = '" + sData[8] + "' \n"
					+       ",申込者電話２ = '" + sData[9] + "' \n"
					+       ",申込者電話３ = '" + sData[10] + "' \n"
					+       ",申込者電話 = '" + sData[11] + "' \n"
					+       ",申込者ＦＡＸ１ = '" + sData[12] + "' \n"
					+       ",申込者ＦＡＸ２ = '" + sData[13] + "' \n"
					+       ",申込者ＦＡＸ３ = '" + sData[14] + "' \n"
					+       ",設置場所区分 = '" + sData[15] + "' \n"
					+       ",設置場所カナ = '" + sData[16] + "' \n"
					+       ",設置場所名 = '" + sData[17] + "' \n"
					+       ",設置場所郵便番号 = '" + sData[18] + "' \n"
					+       ",設置場所県ＣＤ = '" + sData[19] + "' \n"
					+       ",設置場所住所１ = '" + sData[20] + "' \n"
					+       ",設置場所住所２ = '" + sData[21] + "' \n"
					+       ",設置場所電話１ = '" + sData[22] + "' \n"
					+       ",設置場所電話２ = '" + sData[23] + "' \n"
					+       ",設置場所電話３ = '" + sData[24] + "' \n"
					+       ",設置場所ＦＡＸ１ = '" + sData[25] + "' \n"
					+       ",設置場所ＦＡＸ２ = '" + sData[26] + "' \n"
					+       ",設置場所ＦＡＸ３ = '" + sData[27] + "' \n"
					+       ",設置場所担当者名 = '" + sData[28] + "' \n"
					+       ",設置場所役職名 = '" + sData[29] + "' \n"
					+       ",設置場所使用料 =  " + sData[30] + "  \n"
					+       ",会員ＣＤ = '" + sData[31] + "' \n"
					+       ",使用開始日 = '" + sData[32] + "' \n"
					+       ",部門ＣＤ = '" + sData[33] + "' \n"
					+       ",部門名 = '" + sData[34] + "' \n"
					+       ",サーマル台数 =  " + sData[35] + "  \n"
					+       ",利用者ＣＤ = '" + sData[36] + "' \n"
					+       ",利用者名 = '" + sData[37] + "' \n"
					+       ",パスワード = '" + sData[38] + "' \n"
					+       ",承認状態ＦＧ = '" + sData[39] + "' \n"
					+       ",メモ = '" + sData[40] + "' \n"
					+       ",更新日時 = " + s更新日時 + " \n"
					+       ",更新ＰＧ = '申込更新' \n"
					+       ",更新者 = '" + sData[42] + "' \n"
					+ " WHERE 管理番号 = '" + sData[0] + "' \n"
					+   " AND 削除ＦＧ = '0' \n"
					+   " AND 更新日時 = " + sData[41] + " \n";

				if (CmdUpdate(sUser, conn2, cmdQuery) != 0)
				{
					string sRet会員   = "";
					string sRet部門   = "";
					string sRet利用者 = "";
					//承認状態ＦＧが[3：承認済]の場合
					if(sData[39].Equals("3"))
					{
						sRet会員 = Ins_Member2(sUser, conn2, sData, s更新日時);
						if(sRet会員.Length == 4)
						{
							//部門マスタ追加
							sRet部門 = Ins_Section2(sUser, conn2, sData, s更新日時);
							if(sRet部門.Length == 4)
							{
								//利用者マスタ追加
								sRet利用者 = Ins_User2(sUser, conn2, sData, s更新日時);
							}
						}
					}
					if(sRet会員.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "お客様：" + sRet会員;
					}
					else if(sRet部門.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "セクション：" + sRet部門;
					}
					else if(sRet利用者.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "ユーザー：" + sRet利用者;
					}
					else
					{
						tran.Commit();
						sRet[0] = "正常終了";
						sRet[1] = s更新日時;
					}
				}
				else
				{
					tran.Rollback();
					sRet[0] = "他の端末で更新されています";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 自動出荷登録用住所取得３
		 * 　　ＳＭ０２荷受人、ＣＭ１４郵便番号、ＣＭ１５着店非表示、ＣＭ１９郵便住所
		 *     の３マスタを使用して着店コードを決定する。
		 * 引数：会員コード、部門コード、荷受人コード、郵便番号、住所、氏名
		 * 戻値：ステータス、店所ＣＤ、店所名、住所ＣＤ
		 *
		 * Create : 2008.06.12 kcl)森本
		 * 　　　　　　Get_autoEntryPref を元に作成
		 * Modify : 2008.12.25 kcl)森本
		 *            引数に氏名を追加
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2syukka\Service1.asmx.cs(5201):
		*/
		[WebMethod]
		public string [] Get_autoEntryPref3(string [] sUser, 
			string sKaiinCode, string sBumonCode, string sNiukeCode, 
			string sYuubin, string sJuusyo, string sShimei)
		{
			// ログ出力
			logWriter(sUser, INF, "住所取得３開始");

			OracleConnection conn2 = null;
			string [] sRet = new string [4];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				// ＤＢ接続に失敗
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			try 
			{
				// 着店コードの取得
				string [] sResult = this.Get_tyakuten3(sUser, conn2, sKaiinCode, sBumonCode, sNiukeCode, sYuubin, sJuusyo, sShimei);

				if (sResult[0] == " ")
				{
					// 取得成功
					sRet[1] = sResult[3];	// 住所ＣＤ
					sRet[2] = sResult[1];	// 店所ＣＤ
					sRet[3] = sResult[2];	// 店所名

					sRet[0] = "正常終了";
				}
				else
				{
					// 取得失敗
					sRet[0] = "該当データがありません";
				}

				// ログ出力
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				// Oracle のエラー
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				// それ以外のエラー
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				// 終了処理
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}
		// MOD 2011.06.06 東都）高木 王子運送輸送商品コード検索追加 START
		/*********************************************************************
		 * 輸送商品コード検索
		 * 引数：部門ＣＤ、記事
		 * 戻値：ステータス
		 *       輸送商品名から輸送商品コードを検索します
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2kiji\Service1.asmx.cs(571):
		*/
		[WebMethod]
		public String[] Get_kijiCD(string[] sUser, string sBcode, string sKname)
		{
			logWriter(sUser, INF, "輸送商品コード検索開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string sKcode = "";

				if(sBcode.Equals("100"))
				{
					if(sKname.StartsWith("時間指定"))
					{
						if(sKname.EndsWith("まで"))
						{
							sKcode = "11X";
						}
						else if(sKname.EndsWith("以降"))
						{
							sKcode = "12X";
						}
					}
				}
				else if(sBcode.Equals("200"))
				{
					if(sKname.StartsWith("時間指定"))
					{
						if(sKname.EndsWith("まで"))
						{
							sKcode = "21X";
						}
						else if(sKname.EndsWith("以降"))
						{
							sKcode = "22X";
						}
					}
				}

				sbQuery.Append( "SELECT 記事ＣＤ" );
				sbQuery.Append(  " FROM ＳＭ０３記事 \n" );
				sbQuery.Append( " WHERE 会員ＣＤ = 'Jyusoshohin' \n" ); // 王子運送対応
				sbQuery.Append(   " AND 部門ＣＤ = '" + sBcode +"' \n" );
				if (sKcode.Length != 0)
				{
					sbQuery.Append(   " AND 記事ＣＤ = '" + sKcode +"' \n" );
				}
				else
				{
					sbQuery.Append(   " AND 記事     = '" + sKname +"' \n" );
				}
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				if(reader.Read())
				{
					sRet[0] = "正常終了";
					sRet[1] = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "該当データがありません";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 記事印刷データ取得
		 * 引数：会員ＣＤ、部門ＣＤ
		 * 戻値：ステータス、記事ＣＤ、記事
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2print\Service1.asmx.cs(1520):
		*/
		[WebMethod]
		public ArrayList Get_NotePrintData(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "記事印刷データ取得開始");

			OracleConnection conn2 = null;
			ArrayList alRet = new ArrayList();
			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				alRet.Add(sRet);
				return alRet;
			}

			try
			{
				//輸送指示の取得
				System.Text.StringBuilder cmdQuery_y = new System.Text.StringBuilder(256);
				cmdQuery_y.Append("SELECT ");
				cmdQuery_y.Append(" SM03_1.記事ＣＤ ");
				cmdQuery_y.Append(",SM03_1.記事 ");
				cmdQuery_y.Append(",NVL(SM03_2.記事ＣＤ, ' ') ");
				cmdQuery_y.Append(",NVL(SM03_2.記事, ' ') ");
				cmdQuery_y.Append(" FROM \"ＳＭ０３記事\" SM03_1 ");
				cmdQuery_y.Append(" LEFT JOIN \"ＳＭ０３記事\" SM03_2 ");
				cmdQuery_y.Append(       " ON SM03_1.会員ＣＤ = SM03_2.会員ＣＤ ");
				cmdQuery_y.Append(      " AND SM03_1.記事ＣＤ = SM03_2.部門ＣＤ ");
				cmdQuery_y.Append(      " AND '0'             = SM03_2.削除ＦＧ ");
				cmdQuery_y.Append("WHERE SM03_1.会員ＣＤ   = 'Jyusoshohin' "); // 王子運送対応
				cmdQuery_y.Append(  "AND SM03_1.部門ＣＤ   = '0000' ");
				cmdQuery_y.Append(  "AND SM03_1.削除ＦＧ   = '0' ");
				cmdQuery_y.Append("ORDER BY SM03_1.記事ＣＤ,SM03_2.記事ＣＤ \n");
				OracleDataReader reader_y = CmdSelect(sUser, conn2, cmdQuery_y);

				//品名記事の取得
				System.Text.StringBuilder cmdQuery_h = new System.Text.StringBuilder(256);
				cmdQuery_h.Append("SELECT ");
				cmdQuery_h.Append(" 記事ＣＤ ");
				cmdQuery_h.Append(",記事 ");
				cmdQuery_h.Append(" FROM \"ＳＭ０３記事\" ");
				cmdQuery_h.Append("WHERE 会員ＣＤ   = '" + sKey[0] + "' ");
				cmdQuery_h.Append(  "AND 部門ＣＤ   = '" + sKey[1] + "' ");
				cmdQuery_h.Append(  "AND 削除ＦＧ   = '0' ");
				cmdQuery_h.Append("ORDER BY 記事ＣＤ \n");
				OracleDataReader reader_h = CmdSelect(sUser, conn2, cmdQuery_h);

				bool b輸送指示 = true;
				bool b品名記事 = true;
				string s親記事 = "";
				while (true)
				{
					if (b輸送指示) b輸送指示 = reader_y.Read();
					if (b品名記事) b品名記事 = reader_h.Read();

					string[] sData = new string[4];
					if (b輸送指示)
					{
						sData[0]  = reader_y.GetString(0).TrimEnd();
						sData[1]  = reader_y.GetString(1).TrimEnd();
					}
					else
					{
						sData[0] = "";
						sData[1] = "";
					}
					if (b輸送指示 && !sData[0].Equals(s親記事))
					{
						if (b品名記事)
						{
							sData[2]  = reader_h.GetString(0).TrimEnd();
							sData[3]  = reader_h.GetString(1).TrimEnd();
						}
						else
						{
							sData[2] = "";
							sData[3] = "";
						}
						s親記事 = sData[0];
						alRet.Add(sData);
						if (!reader_y.GetString(2).TrimEnd().Equals(""))
						{
							sData = new string[4];
							if (b品名記事) b品名記事 = reader_h.Read();
							sData[0]  = "  " + reader_y.GetString(2).TrimEnd();
							sData[1]  = "　　　" + reader_y.GetString(3).TrimEnd();
						}
						else
						{
							continue;
						}
					}
					else
					{
						if (b輸送指示)
						{
							sData[0]  = "  " + reader_y.GetString(2).TrimEnd();
							sData[1]  = "　　　" + reader_y.GetString(3).TrimEnd();
						}
					}

					if (b品名記事)
					{
						sData[2]  = reader_h.GetString(0).TrimEnd();
						sData[3]  = reader_h.GetString(1).TrimEnd();
					}
					else
					{
						sData[2] = "";
						sData[3] = "";
					}
					if (!b輸送指示 && !b品名記事) break;
					alRet.Add(sData);
				}
				disposeReader(reader_y);
				disposeReader(reader_h);
				reader_y = null;
				reader_h = null;
				if (alRet.Count == 0)
				{
					sRet[0] = "該当データがありません";
					alRet.Add(sRet);
				}
				else
				{
					sRet[0] = "正常終了";
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
				sRet[0] = "サーバエラー：" + ex.Message;
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
		// MOD 2011.06.06 東都）高木 王子運送輸送商品コード検索追加 END
		// ADD 2015.05.01 BEVAS) 前田 CM14J郵便番号存在チェック START

		/*********************************************************************
		 * 住所の取得 王子対応版
		 * 引数：郵便番号
		 * 戻値：ステータス、郵便番号、住所、住所ＣＤ
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2address\Service1.asmx.cs(535):
		*/ 
		// ADD 2005.05.11 東都）高木 ORA-03113対策？ START
		private static string GET_BYPOSTCODE2J_SELECT
			= "SELECT 郵便番号, 都道府県名, 市区町村名, 町域名, \n"
			+ " 都道府県ＣＤ, 市区町村ＣＤ, 大字通称ＣＤ \n"
			+ " FROM ＣＭ１４郵便番号Ｊ \n";
		// ADD 2005.05.11 東都）高木 ORA-03113対策？ END
		[WebMethod]
		public String[] Get_byPostcode2(string[] sUser, string s郵便番号)
		{
			// DEL 2007.05.10 東都）高木 未使用関数のコメント化
			//			logFileOpen(sUser);
			logWriter(sUser, INF, "住所取得開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ADD-S 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				// DEL 2007.05.10 東都）高木 未使用関数のコメント化
				//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			// DEL 2007.05.10 東都）高木 未使用関数のコメント化
			//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
			//			// 会員チェック
			//			sRet[0] = userCheck2(conn2, sUser);
			//			if(sRet[0].Length > 0)
			//			{
			//				disconnect2(sUser, conn2);
			//				logFileClose();
			//				return sRet;
			//			}
			//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				cmdQuery
					// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
					//					= "SELECT 郵便番号, TRIM(都道府県名), TRIM(市区町村名), TRIM(町域名), \n"
					//					+        "都道府県ＣＤ || 市区町村ＣＤ || 大字通称ＣＤ \n"
					//					+   " FROM ＣＭ１４郵便番号Ｊ \n";
					= GET_BYPOSTCODE2J_SELECT;
				// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
				if(s郵便番号.Length == 7)
				{
					cmdQuery += " WHERE 郵便番号 = '" + s郵便番号 + "' \n";
				}
				else
				{
					cmdQuery += " WHERE 郵便番号 LIKE '" + s郵便番号 + "%' \n";
				}
				cmdQuery +=    " AND 削除ＦＧ = '0' \n";

				// MOD-S 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
				//OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				logWriter(sUser, INF_SQL, "###バインド後（想定）###\n" + cmdQuery);	//修正前のUPDATE文をログ出力

				cmdQuery = GET_BYPOSTCODE2J_SELECT;
				if(s郵便番号.Length == 7)
				{
					cmdQuery += " WHERE 郵便番号 = :p_YuubinNo \n";
				}
				else
				{
					cmdQuery += " WHERE 郵便番号 LIKE :p_YuubinNo \n";
				}
				cmdQuery +=    " AND 削除ＦＧ = '0' \n";

				wk_opOraParam = new OracleParameter[1];
				if(s郵便番号.Length == 7)
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s郵便番号, ParameterDirection.Input);
				}
				else
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s郵便番号+"%", ParameterDirection.Input);
				}

				OracleDataReader	reader = CmdSelect(sUser, conn2, cmdQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）

				if (reader.Read())
				{
					// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
					//					sRet[1] = reader.GetString(0);	// 郵便番号
					//					sRet[2] = reader.GetString(1)	// 都道府県名
					//							+ reader.GetString(2)	// 市区町村名
					//							+ reader.GetString(3);	// 町域名
					//					sRet[3] = reader.GetString(4);	// 住所ＣＤ
					sRet[1] = reader.GetString(0).Trim();	// 郵便番号
					sRet[2] = reader.GetString(1).Trim()	// 都道府県名
						+ reader.GetString(2).Trim()	// 市区町村名
						+ reader.GetString(3).Trim();	// 町域名
					sRet[3] = reader.GetString(4).Trim()	// 都道府県ＣＤ
						+ reader.GetString(5).Trim()	// 市区町村ＣＤ
						+ reader.GetString(6).Trim();	// 大字通称ＣＤ
					// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
					sRet[0] = "正常終了";
				}
				else
				{
					sRet[0] = "該当データがありません";
				}
				// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
				// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
				// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
				// DEL 2007.05.10 東都）高木 未使用関数のコメント化
				//				logFileClose();

			}
			return sRet;
		}

		/*********************************************************************
		 * アップロードデータ追加２ 王子対応
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2otodoke\Service1.asmx.cs(1209):
		*/ 
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM ＣＭ１４郵便番号Ｊ \n"
			;

		[WebMethod]
		public String[] otodoke_Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "お届先アップロードデータ追加２開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try{
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow+1] = "";

					string[] sData = sList[iRow].Split(',');
					string s住所ＣＤ = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						s住所ＣＤ = sData[21];
					}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
					string s特殊ＣＤ = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s特殊ＣＤ = sData[19];
//					}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END

//					sData[15] = sData[15].TrimEnd();
//					if(sData[15].Length == 0){
//						sRet[iRow+1] = "郵未";//未設定
//						continue;
//					}
//					if(sData[15].Length != 7){
//						sRet[iRow+1] = "郵桁";//桁数に誤りがある場合
//						continue;
//					}

					//郵便番号マスタの存在チェック
					OracleDataReader reader;
					string cmdQuery = "";
					cmdQuery = INS_UPLOADDATA2_SELECT1
							+ "WHERE 郵便番号 = '" + sData[15] + "' \n"
//保留 MOD 2010.04.13 東都）高木 郵便番号が削除された時の障害対応 START
							+ "AND 削除ＦＧ = '0' \n"
//保留 MOD 2010.04.13 東都）高木 郵便番号が削除された時の障害対応 END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow+1] = sData[15];//該当データ無し
						reader.Close();
						disposeReader(reader);
						reader = null;
						continue;
					}
					reader.Close();

					cmdQuery
						= "SELECT 削除ＦＧ \n"
						+   "FROM ＳＭ０２荷受人 \n"
						+  "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
						+    "AND 部門ＣＤ = '" + sData[1] + "' \n"
						+    "AND 荷受人ＣＤ = '" + sData[2] + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s削除ＦＧ = "";
					while (reader.Read()){
						s削除ＦＧ = reader.GetString(0);
						iCnt++;
					}
					reader.Close();

					if(iCnt == 1){
						//追加
						cmdQuery 
							= "INSERT INTO ＳＭ０２荷受人 \n"
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
							+           "'" + s住所ＣＤ + "', "
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//							+           "' ', \n" //特殊ＣＤ
							+           "'" + s特殊ＣＤ + "', \n"
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							+           "' ', \n"
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'お届取込', \n"
							+           "'" + sData[20] + "')"
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//上書き更新
						cmdQuery
							= "UPDATE ＳＭ０２荷受人 \n"
							+    "SET 電話番号１ = '" + sData[3] + "' "
							+       ",電話番号２ = '" + sData[4] + "' \n"
							+       ",電話番号３ = '" + sData[5] + "' "
							+       ",ＦＡＸ番号１ = '" + sData[6] + "' \n"
							+       ",ＦＡＸ番号２ = '" + sData[7] + "' "
							+       ",ＦＡＸ番号３ = '" + sData[8] + "' \n"
							+       ",住所１ = '" + sData[9] + "' "
							+       ",住所２ = '" + sData[10] + "' \n"
							+       ",住所３ = '" + sData[11] + "' "
							+       ",名前１ = '" + sData[12] + "' \n"
							+       ",名前２ = '" + sData[13] + "' "
							+       ",名前３ = '" + sData[14] + "' \n"
							+       ",郵便番号 = '" + sData[15] + "' "
							+       ",住所ＣＤ = '" + s住所ＣＤ + "' \n"
							+       ",カナ略称 = '" + sData[16] + "' "
							+       ",一斉出荷区分 = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)森本 着店コード検索方法の変更 START
//							+       ",特殊ＣＤ = ' ' \n" //特殊ＣＤ
							+       ",特殊ＣＤ = '" + s特殊ＣＤ + "' \n"
// ADD 2008.06.13 kcl)森本 着店コード検索方法の変更 END
							+       ",特殊計 = '" + sData[18] + "' \n"
							+       ",メールアドレス = ' ' "
							+       ",削除ＦＧ = '0' \n"
							+       ",登録ＰＧ = ' ' \n"
							;
						if(s削除ＦＧ == "1"){
							cmdQuery
								+=  ",登録日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",登録者 = '" + sData[20] + "' \n"
								;
						}
						cmdQuery
							+=      ",更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",更新ＰＧ = 'お届取込' "
							+       ",更新者 = '" + sData[20] + "' \n"
							+ "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
							+   "AND 部門ＣＤ = '" + sData[1] + "' \n"
							+   "AND 荷受人ＣＤ = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "正常終了");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

// MOD 2010.09.08 東都）高木 ＣＳＶ取込機能の追加 START
		/*********************************************************************
		 * アップロードデータ追加２  王子　ご依頼主登録
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		/* 秀丸で下の行にカーソルをもっていき[F10]キーを押すと元ソースが参照できます
		..\is2goirai\Service1.asmx.cs(1698):
		*/
 		private static string goirai_INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM ＣＭ１４郵便番号Ｊ \n"
			;

		private static string goirai_INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
//			= "SELECT 郵便番号 \n"
//			+ " FROM ＣＭ０２部門 \n"
			= "SELECT CM02.郵便番号 \n"
			+ ", NVL(CM01.保留印刷ＦＧ,'0') \n"
			+ " FROM ＣＭ０２部門 CM02 \n"
			+ " LEFT JOIN ＣＭ０１会員 CM01 \n"
			+ " ON CM02.会員ＣＤ = CM01.会員ＣＤ \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
			;

		private static string goirai_INS_UPLOADDATA2_SELECT3
			= "SELECT 1 \n"
			+ " FROM ＳＭ０４請求先 \n"
			;

		[WebMethod]
		public String[] goirai_Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "ご依頼主アップロードデータ追加２開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[(sList.Length*2) + 1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();
			OracleDataReader reader;
			string cmdQuery = "";

			sRet[0] = "";
			try{
				string s部郵便番号 = "";
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				string s重量入力制御 = "0";
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow*2+1] = "";
					sRet[iRow*2+2] = "";

					string[] sData = sList[iRow].Split(',');
					if(sData.Length != 21){
						throw new Exception("パラメータ長エラー["+sData.Length+"]");
					}

					string s会員ＣＤ   = sData[0];
					string s部門ＣＤ   = sData[1];
					string s荷送人ＣＤ = sData[2];
					string s郵便番号   = sData[12];
					string s請求先ＣＤ = sData[17];
					string s請求先部課 = sData[18];

					if(iRow == 0){
						//部門マスタの存在チェック
						cmdQuery = goirai_INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
//								+ "WHERE 会員ＣＤ = '" + s会員ＣＤ + "' \n"
//								+ "AND 部門ＣＤ = '" + s部門ＣＤ + "' \n"
//								+ "AND 削除ＦＧ = '0' \n"
								+ "WHERE CM02.会員ＣＤ = '" + s会員ＣＤ + "' \n"
								+ "AND CM02.部門ＣＤ = '" + s部門ＣＤ + "' \n"
								+ "AND CM02.削除ＦＧ = '0' \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
								;

						reader = CmdSelect(sUser, conn2, cmdQuery);
						if(!reader.Read()){
							reader.Close();
							disposeReader(reader);
							reader = null;
							throw new Exception("セクションが存在しません");
						}
						s部郵便番号 = reader.GetString(0).TrimEnd();
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
						s重量入力制御 = reader.GetString(1).TrimEnd();
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						reader.Close();
						disposeReader(reader);
						reader = null;
					}

					//郵便番号マスタの存在チェック
					cmdQuery = goirai_INS_UPLOADDATA2_SELECT1
// MOD 2010.09.29 東都）高木 郵便番号(__)対応（＊既存バグだが導入） START
//							+ "WHERE 郵便番号 = '" + s郵便番号 + "' \n"
//							+ "AND 削除ＦＧ = '0' \n"
							;
							string s郵便番号１ = "";
							string s郵便番号２ = "";
							if(s郵便番号.Length > 3){
								s郵便番号１ = s郵便番号.Substring(0,3).Trim();
								s郵便番号２ = s郵便番号.Substring(3).Trim();
								s郵便番号 = s郵便番号１ + s郵便番号２;
							}
							if(s郵便番号.Length == 7){
								cmdQuery += " WHERE 郵便番号 = '" + s郵便番号 + "' \n";
							}else{
								cmdQuery += " WHERE 郵便番号 LIKE '" + s郵便番号 + "%' \n";
							}
							cmdQuery += "AND 削除ＦＧ = '0' \n"
// MOD 2010.09.29 東都）高木 郵便番号(__)対応（＊既存バグだが導入） END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+1] = s郵便番号.TrimEnd();//該当データ無し
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					//請求先マスタの存在チェック
					cmdQuery = goirai_INS_UPLOADDATA2_SELECT3
							+ "WHERE 郵便番号 = '" + s部郵便番号 + "' \n"
							+ "AND 得意先ＣＤ = '" + s請求先ＣＤ + "' \n"
							+ "AND 得意先部課ＣＤ = '" + s請求先部課 + "' \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
							+ "AND 会員ＣＤ = '" + s会員ＣＤ + "' \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
 							+ "AND 削除ＦＧ = '0' \n"
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+2] = s請求先ＣＤ.TrimEnd(); //該当データ無し
						if(s請求先部課.TrimEnd().Length > 0){
							sRet[iRow*2+2] += "-" + s請求先部課.TrimEnd();
						}
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;
					
					//エラーがあれば、次の行
					if(sRet[iRow*2+1].Length != 0 || sRet[iRow*2+2].Length != 0){
						continue;
					}

					cmdQuery
						= "SELECT 削除ＦＧ \n"
						+   "FROM ＳＭ０１荷送人 \n"
						+  "WHERE 会員ＣＤ = '" + s会員ＣＤ + "' \n"
						+    "AND 部門ＣＤ = '" + s部門ＣＤ + "' \n"
						+    "AND 荷送人ＣＤ = '" + s荷送人ＣＤ + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s削除ＦＧ = "";
					while (reader.Read()){
						s削除ＦＧ = reader.GetString(0);
						iCnt++;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					if(iCnt == 1){
						//追加
						cmdQuery 
							= "INSERT INTO ＳＭ０１荷送人 \n"
							+ "VALUES ( \n"
							+  "'" + sData[0] + "', "		//会員ＣＤ
							+  "'" + sData[1] + "', \n"		//部門ＣＤ
							+  "'" + sData[2] + "', \n"		//荷送人ＣＤ

							+  "'" + sData[17] + "', "		//得意先ＣＤ
							+  "'" + sData[18] + "', \n"	//得意先部課ＣＤ
							+  "'" + sData[3] + "', "		//電話番号
							+  "'" + sData[4] + "', "
							+  "'" + sData[5] + "', \n"
							+  "' ', "						//ＦＡＸ番号
							+  "' ', "
							+  "' ', \n"
							+  "'" + sData[6] + "', "		//住所
							+  "'" + sData[7] + "', "
							+  "'" + sData[8] + "', \n"
							+  "'" + sData[9] + "', "		//名前
							+  "'" + sData[10] + "', "
							+  "'" + sData[11] + "', \n"
							+  "'" + sData[12] + "', "		//郵便番号
							+  "'" + sData[13] + "', \n"	//カナ略称
							+  " " + sData[14] + " , "		//才数
							+  " " + sData[15] + " , \n"	//重量
							+  "' ', "						//荷札区分
							+  "'" + sData[16] + "', \n"	//メールアドレス
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
						//上書き更新
						cmdQuery
							= "UPDATE ＳＭ０１荷送人 \n"
							+    "SET 得意先ＣＤ = '" + sData[17] + "' \n"
							+       ",得意先部課ＣＤ = '" + sData[18] + "' \n"
							+       ",電話番号１ = '" + sData[3] + "' \n"
							+       ",電話番号２ = '" + sData[4] + "' \n"
							+       ",電話番号３ = '" + sData[5] + "' \n"
							+       ",ＦＡＸ番号１ = ' ' \n"
							+       ",ＦＡＸ番号２ = ' ' \n"
							+       ",ＦＡＸ番号３ = ' ' \n"
							+       ",住所１ = '" + sData[6] + "' \n"
							+       ",住所２ = '" + sData[7] + "' \n"
							+       ",住所３ = '" + sData[8] + "' \n"
							+       ",名前１ = '" + sData[9] + "' \n"
							+       ",名前２ = '" + sData[10] + "' \n"
							+       ",名前３ = '" + sData[11] + "' \n"
							+       ",郵便番号 = '" + sData[12] + "' "
							+       ",カナ略称 = '" + sData[13] + "' "
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
							;
						if(s重量入力制御 == "1"){
							cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
							+       ",才数 = "+ sData[14] +" "
							+       ",重量 = "+ sData[15] +" "
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
							;
						}
						cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
							+       ",荷札区分 = ' ' "
							+       ",\"メールアドレス\" = '"+ sData[16] +"' "
							+       ",削除ＦＧ = '0' \n"
							;
						if(s削除ＦＧ == "1"){
							cmdQuery
								+=  ",登録日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",登録ＰＧ = '" + sData[19] + "' "
								+   ",登録者 = '" + sData[20] + "' \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
								;
							if(s重量入力制御 != "1"){
								cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
								+   ",才数 = "+ sData[14] +" "
								+   ",重量 = "+ sData[15] +" \n"
								;
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
							}
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						}
						cmdQuery
							+=      ",更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",更新ＰＧ = '" + sData[19] + "' "
							+       ",更新者 = '" + sData[20] + "' \n"
							+ "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
							+   "AND 部門ＣＤ = '" + sData[1] + "' \n"
							+   "AND 荷送人ＣＤ = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "正常終了");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 東都）高木 ＣＳＶ取込機能の追加 END

// ADD 2015.05.01 BEVAS) 前田 CM14J郵便番号存在チェック END
	}

}
