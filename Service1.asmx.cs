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
	// �C������
	//--------------------------------------------------------------------------
	// 2010.12.14 ACT�j�_�� �V�K�쐬
	//--------------------------------------------------------------------------
	// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� 
	//--------------------------------------------------------------------------
	// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� 
	// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� 
	// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ 
	// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή�
	// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� 
	// MOD 2011.06.01 ���s�j���� �r�p�k�̒��� 
	// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� 
	// MOD 2011.10.06 ���s�j���� �o�׃f�[�^�̈�����O�̒ǉ� 
	// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ 
	//--------------------------------------------------------------------------
	// MOD 2015.05.01 BEVAS) �O�c CM14J�X�֔ԍ����݃`�F�b�N
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
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			connectService();
		}

		#region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
		
		//Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
		private IContainer components = null;
				
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
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
		 * ���X�擾
		 * �����F�ב��l�b�c
		 * �ߒl�F�X�e�[�^�X�A�X���b�c�A�X�����A�s���{���b�c�A�s�撬���b�c�A�厚�ʏ̂b�c
		 *
		 *********************************************************************/
		private static string GET_HATUTEN3_SELECT
			= "SELECT CM14.�X���b�c \n"
			+  " FROM �b�l�O�Q���� CM02 \n"
			+      ", �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
			;

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2init\Service1.asmx.cs(3062):
		*/
		[WebMethod]
		public String[] Get_hatuten3(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "���X�擾�R�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_HATUTEN3_SELECT);
				sbQuery.Append(" WHERE CM02.����b�c = '" + sKcode + "' \n");
				sbQuery.Append(" AND CM02.����b�c = '" + sBcode + "' \n");
				sbQuery.Append(" AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				if(reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();

					if (sRet[1].Equals("999")) // ���q�^���Ή�
					{
						sRet[0] = "�w�肵���Z���́A�z�B�s�\�G���A�ł�";
					}
					else
					{
						sRet[0] = "����I��";
					}
				}
				else
				{
					sRet[0] = "���p�҂̏W�דX�擾�Ɏ��s���܂���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^����
		 * �����F����b�c�A����b�c
		 * �ߒl�F�X�e�[�^�X�A����b�c�A���喼�A�o�͏��A�X�����A�X�V����
		 *
		 * �Q�ƌ��F����}�X�^.cs 2��
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(717):
		*/
		[WebMethod]
		public string[] Sel_Section(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "����}�X�^�����J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[19];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT CM02.����b�c "
					+      ", CM02.���喼 "
					+      ", CM02.�o�͏� "
					+      ", CM02.�X�֔ԍ� "
					+      ", NVL(CM10.�X����, ' ') "
					+      ", CM02.�ݒu��Z���P "
					+      ", CM02.�ݒu��Z���Q "
					+      ", CM02.�X�V���� \n"
					+      ", CM02.�T�[�}���䐔 \n"
					+      ", NVL(CM06.�V���A���ԍ��P,' ') \n"
					+      ", NVL(CM06.��ԂP,' ') \n"
					+      ", NVL(CM06.�V���A���ԍ��Q,' ') \n"
					+      ", NVL(CM06.��ԂQ,' ') \n"
					+      ", NVL(CM06.�V���A���ԍ��R,' ') \n"
					+      ", NVL(CM06.��ԂR,' ') \n"
					+      ", NVL(CM06.�V���A���ԍ��S,' ') \n"
					+      ", NVL(CM06.��ԂS,' ') \n"
					+      ", NVL(CM06.�g�p��,0) \n"
					+  " FROM �b�l�O�Q���� CM02 \n"
					+      " LEFT JOIN �b�l�O�U����g�� CM06 \n"
					+      " ON CM02.����b�c = CM06.����b�c \n"
					+      " AND CM02.����b�c = CM06.����b�c \n"
					+  " LEFT JOIN �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
					+    " ON CM02.�X�֔ԍ� = CM14.�X�֔ԍ� "
					+  " LEFT JOIN �b�l�P�O�X�� CM10 \n"
					+    " ON CM14.�X���b�c = CM10.�X���b�c "
					+   " AND CM10.�폜�e�f = '0' \n"
					+ " WHERE CM02.����b�c = '" + sKey[0] + "' \n"
					+   " AND CM02.����b�c = '" + sKey[1] + "' \n"
					+   " AND CM02.�폜�e�f = '0' \n"
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
					sRet[0] = "�Y���f�[�^������܂���";
				else
					sRet[0] = "����I��";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ������}�X�^�ꗗ�擾
		 * �����F�X���b�c
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i�X�֔ԍ��A���Ӑ�b�c�j...
		 *
		 * �Q�ƌ��F������}�X�^.cs ���ݖ��g�p
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(2797):
		*/
		[WebMethod]
		public string[] Get_Claim(string[] sUser, string sKey)
		{
			logWriter(sUser, INF, "������}�X�^�ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(SM04.�X�֔ԍ�) || '|' "
					+     "|| TRIM(SM04.���Ӑ�b�c)     || '|' "
					+     "|| TRIM(SM04.���Ӑ敔�ۂb�c) || '|' "
					+     "|| TRIM(SM04.���Ӑ敔�ۖ�)   || '|' "
					+     "|| TRIM(SM04.����b�c) || '|' "
					+     "|| NVL(CM01.�����, ' ')  || '|' "
					+     "|| TO_CHAR(SM04.�X�V����) || '|' \n"
					+  " FROM �b�l�P�S�X�֔ԍ��i CM14 " // ���q�^���Ή�
					+      ", �r�l�O�S������ SM04 \n"
					+  " LEFT JOIN �b�l�O�P��� CM01 \n"
					+    " ON SM04.����b�c = CM01.����b�c "
					+    "AND '0' = CM01.�폜�e�f \n"
					+ " WHERE CM14.�X���b�c = '" + sKey + "' \n"
					+   " AND CM14.�X�֔ԍ� = SM04.�X�֔ԍ� \n"
					+   " AND SM04.�폜�e�f = '0' \n"
					+ " ORDER BY SM04.����b�c "
					+          ",SM04.���Ӑ�b�c "
					+          ",SM04.���Ӑ敔�ۂb�c \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ������}�X�^�ꗗ�擾�Q
		 * �����F�X���b�c�A����b�c
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i�X�֔ԍ��A���Ӑ�b�c�j...
		 *
		 * �Q�ƌ��F������}�X�^.cs
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(2908):
		*/
		[WebMethod]
		public string[] Get_Claim2(string[] sUser, string sTensyo, string sKaiin)
		{
			logWriter(sUser, INF, "������}�X�^�ꗗ�擾�Q�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(SM04.�X�֔ԍ�) || '|' "
					+     "|| TRIM(SM04.����b�c) || '|' "
					+     "|| NVL(TRIM(CM01.�����), ' ')  || '|' "
					+     "|| TRIM(SM04.���Ӑ�b�c)     || '|' "
					+     "|| TRIM(SM04.���Ӑ敔�ۂb�c) || '|' "
					+     "|| TRIM(SM04.���Ӑ敔�ۖ�)   || '|' "
					+     "|| TO_CHAR(SM04.�X�V����) || '|' \n"
					+  " FROM �b�l�P�S�X�֔ԍ��i CM14 " // ���q�^���Ή�
					+      ", �r�l�O�S������ SM04 \n"
					+  " LEFT JOIN �b�l�O�P��� CM01 \n"
					+    " ON SM04.����b�c = CM01.����b�c "
					+    "AND '0' = CM01.�폜�e�f \n"
					+ " WHERE CM14.�X���b�c = '" + sTensyo + "' \n";

				if(sKaiin.Length > 0)
				{
					cmdQuery += "AND  SM04.����b�c = '" + sKaiin + "' \n";
				}
				cmdQuery
					+=  " AND CM14.�X�֔ԍ� = SM04.�X�֔ԍ� \n"
					+   " AND SM04.�폜�e�f = '0' \n"
					+   " AND CM01.�Ǘ��ҋ敪 IN ('1','3','4') \n" // 1:�Ǘ��� 3:���q��� 4:���q�c�Ə�
					+ " ORDER BY SM04.����b�c "
					+          ",SM04.���Ӑ�b�c "
					+          ",SM04.���Ӑ敔�ۂb�c \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0));
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �X�֔ԍ��}�X�^�擾
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�X����
		 *
		 * �Q�ƌ��F����}�X�^.cs
		 * �Q�ƌ��F������}�X�^.cs
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(3668):
		*/
		[WebMethod]
		public string[] Sel_Postcode(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "�X�֔ԍ��}�X�^�����J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[4]{"","","",""};

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT NVL(CM10.�X����, ' '), \n"
					+ " TRIM(CM14.�s���{����) || TRIM(CM14.�s�撬����) || TRIM(CM14.���於) \n"
					+ ", CM14.�X���b�c \n"
					+  " FROM �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
					+  " LEFT JOIN �b�l�P�O�X�� CM10 \n"
					+    " ON CM14.�X���b�c = CM10.�X���b�c "
					+    "AND CM10.�폜�e�f = '0' \n"
					+ " WHERE CM14.�X�֔ԍ� = '" + sKey[0] + "' \n"
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
					sRet[0] = "�Y���f�[�^������܂���";
				}
				else
				{
					if (sRet[3].Equals("999")) // ���q�^���Ή�
					{
						sRet[0] = "�w�肵���Z���́A�z�B�s�\�G���A�ł�";
					}
					else
					{
						sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �X�֔ԍ��}�X�^�擾
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�Z���A�X���������A�X���b�c
		 *
		 * �Q�ƌ��F�������.cs		[]	
		 * �Q�ƌ��F�X�����.cs		[]	
		 * �Q�ƌ��F������}�X�^.cs	[]	
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(7318):
		*/
		[WebMethod]
		public string[] Sel_Postcode1(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "�X�֔ԍ��}�X�^�����J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT NVL(CM10.�X����, ' '), \n"
					+ " TRIM(CM14.�s���{����) || TRIM(CM14.�s�撬����) || TRIM(CM14.���於),NVL(TRIM(CM10.�X��������), ' '),TRIM(CM14.�X���b�c) \n"
					+  " FROM �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
					+  " LEFT JOIN �b�l�P�O�X�� CM10 \n"
					+    " ON CM14.�X���b�c = CM10.�X���b�c "
					+    "AND CM10.�폜�e�f = '0' \n"
					+ " WHERE CM14.�X�֔ԍ� = '" + sKey[0] + "' \n"
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
					sRet[0] = "�Y���f�[�^������܂���";
				}
				else
				{
					if (sRet[4].Equals("999")) // ���q�^���Ή�
					{
						sRet[0] = "�w�肵���Z���́A�z�B�s�\�G���A�ł�";
					}
					else
					{
						sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ���O�C���F��
		 * �����F����b�c�A���p�҂b�c�A�p�X���[�h
		 * �ߒl�F�X�e�[�^�X�A����b�c�A������A���p�҂b�c�A���p�Җ�
		 *
		 *********************************************************************/
		private static string SET_LOGIN_SELECT1
			= "SELECT CM01.����b�c, \n"
			+ " CM01.�����, \n"
			+ " CM04.���p�҂b�c, \n"
			+ " CM04.���p�Җ� \n"
			+ ", CM01.�Ǘ��ҋ敪 \n"
			+ ", NVL(CM14.�X���b�c,' ') \n"
			+ " FROM �b�l�O�P��� CM01, \n"
			+ " �b�l�O�Q���� CM02, \n"
			+ " �b�l�P�S�X�֔ԍ��i CM14, \n" // ���q�^���Ή�
			+ " �b�l�O�S���p�� CM04 \n";

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(4657):
		*/
		[WebMethod]
		public string[] Set_login(string[] sUser, string[] sKey) 
		{
			logWriter(sUser, INF, "���O�C���F�؊J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[7];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= SET_LOGIN_SELECT1
					+ " WHERE CM01.����b�c = '" + sKey[0] + "' \n"
					+   " AND CM01.����b�c = CM04.����b�c \n"
					+   " AND CM04.���p�҂b�c = '" + sKey[1] + "' \n"
					+   " AND CM04.�p�X���[�h = '" + sKey[2] + "' \n"
					+   " AND CM01.�g�p�J�n�� <= TO_CHAR(SYSDATE,'YYYYMMDD') \n"
					+   " AND CM01.�g�p�I���� >= TO_CHAR(SYSDATE,'YYYYMMDD') \n"
					+   " AND CM01.�Ǘ��ҋ敪 IN ('1','4') \n" // 1:�Ǘ��� 4:���q�c�Ə�
					+   " AND CM01.�폜�e�f = '0' \n"
					+   " AND CM04.�폜�e�f = '0' \n"
					+   " AND CM04.����b�c = CM02.����b�c \n"
					+   " AND CM04.����b�c = CM02.����b�c \n"
					+   " AND           '0' = CM02.�폜�e�f \n"
					+   " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ�(+) \n"
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
					if (sRet[6].Equals("999")) // ���q�^���Ή�
					{
						sRet[0] = "�w�肵���Z���́A�z�B�s�\�G���A�ł�";
					}
					else
					{
						sRet[0] = "����I��";
					}
				}
				else
				{
					sRet[0] = "�Y���f�[�^������܂���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ������擾�i�b�r�u�o�͗p�j
		 * �����F����b�c�A�g�p�J�n���i�J�n�A�I���j�A�g�p�I�����i�J�n�A�I���j�A
		 *		 ���p�ғo�^���i�J�n�A�I���j
		 * �ߒl�F�X�e�[�^�X�A����b�c�A������A�g�p�J�n��...
		 *
		 * �Q�ƌ��F������b�r�u�o��.cs
		 *********************************************************************/
		private static string GET_KAIINCSV_SELECT
			= "SELECT R.����b�c,NVL(K.�����,' '),NVL(K.�g�p�J�n��,' '),NVL(K.�g�p�I����,' '), \n"
			+       " R.����b�c,NVL(B.���喼,' '),NVL(Y.�X���b�c,' '),NVL(T.�X����,' '), \n"
			+       " NVL(B.�ݒu��Z���P,' '),NVL(B.�ݒu��Z���Q,' '), \n"
			+       " R.���p�҂b�c,R.\"�p�X���[�h\",R.���p�Җ�,SUBSTR(R.�o�^����,1,8) \n"
			+       " ,NVL(B.\"�T�[�}���䐔\",'0')\n"
			+      ", NVL(CM06.�V���A���ԍ��P,' '), DECODE(CM06.��ԂP,'1 ','�ԕi','2 ','�s�Ǖi','3 ','�s��','4 ','���̑�','5 ','������',' ') \n"
			+      ", NVL(CM06.�V���A���ԍ��Q,' '), DECODE(CM06.��ԂQ,'1 ','�ԕi','2 ','�s�Ǖi','3 ','�s��','4 ','���̑�','5 ','������',' ') \n"
			+      ", NVL(CM06.�V���A���ԍ��R,' '), DECODE(CM06.��ԂR,'1 ','�ԕi','2 ','�s�Ǖi','3 ','�s��','4 ','���̑�','5 ','������',' ') \n"
			+      ", NVL(CM06.�V���A���ԍ��S,' '), DECODE(CM06.��ԂS,'1 ','�ԕi','2 ','�s�Ǖi','3 ','�s��','4 ','���̑�','5 ','������',' ') \n"
			+      ", DECODE(K.�Ǘ��ҋ敪,'1','�Ǘ���','3','���q���','4','���q�c�Ə�', K.�Ǘ��ҋ敪) \n"
			+      ", DECODE(K.�L���A�g�e�f,'0',' ','1','�^����\��', K.�L���A�g�e�f) \n"
			+      ", K.�o�^����, K.�X�V���� \n"
			+      ", B.�g�D�b�c, B.�X�֔ԍ�, NVL(CM06.�g�p��,0) \n"
			+      ", DECODE(CM06.����\���Ǘ��ԍ�,NULL,' ',0,' ',TO_CHAR(CM06.����\���Ǘ��ԍ�)) \n"
			+      ", B.�o�^����, B.�X�V���� \n"
			+      ", R.�ב��l�b�c \n"
			+      ", DECODE(R.�����P,' ',' ','1','���x������֎~', R.�����P) \n"
			+      ", R.\"�F�؃G���[��\" \n"
			+      ", R.�o�^�o�f \n"
			+      ", R.�o�^����, R.�X�V���� \n"
			+ " FROM �b�l�O�P��� K,�b�l�O�Q���� B,�b�l�O�S���p�� R,�b�l�P�O�X�� T,�b�l�P�S�X�֔ԍ��i Y \n" // ���q�^���Ή�
			+ " ,�b�l�O�U����g�� CM06 \n"
			;

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(5304):
		*/
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "������b�r�u�o�͗p�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbQuery2 = new StringBuilder(1024);
			try
			{
				sbQuery.Append(" WHERE R.����b�c = K.����b�c \n");
				sbQuery.Append(" AND R.����b�c = B.����b�c \n");
				sbQuery.Append(" AND R.����b�c = B.����b�c \n");
				sbQuery.Append(" AND B.�X�֔ԍ� = Y.�X�֔ԍ�(+) \n");
				sbQuery.Append(" AND Y.�X���b�c = T.�X���b�c(+) \n");
				sbQuery.Append(" AND R.�폜�e�f = '0' \n");
				sbQuery.Append(" AND '0' = K.�폜�e�f \n");
				sbQuery.Append(" AND '0' = B.�폜�e�f \n");
				sbQuery.Append(" AND '0' = T.�폜�e�f(+) \n");
				sbQuery.Append(" AND R.����b�c = CM06.����b�c(+) \n");
				sbQuery.Append(" AND R.����b�c = CM06.����b�c(+) \n");
				sbQuery.Append(" AND K.�Ǘ��ҋ敪 IN ('3','4') \n"); // 3:���q��� 4:���q�c�Ə�

				
				if(sData[0].Length > 0 && sData[1].Length > 0)
					sbQuery.Append(" AND R.����b�c  BETWEEN '"+ sData[0] + "' AND '"+ sData[1] +"' \n");
				else
				{
					if(sData[0].Length > 0 && sData[1].Length == 0)
						sbQuery.Append(" AND R.����b�c = '"+ sData[0] + "' \n");
				}

				if(sData[2].Length > 0 && sData[3].Length > 0)
					sbQuery.Append(" AND K.�g�p�J�n��  BETWEEN '"+ sData[2] + "' AND '"+ sData[3] +"' \n");
				else
				{
					if(sData[2].Length > 0 && sData[3].Length == 0)
						sbQuery.Append(" AND K.�g�p�J�n�� = '"+ sData[2] + "' \n");
				}

				if(sData[4].Length > 0 && sData[5].Length > 0)
					sbQuery.Append(" AND K.�g�p�I����  BETWEEN '"+ sData[4] + "' AND '"+ sData[5] +"' \n");
				else
				{
					if(sData[4].Length > 0 && sData[5].Length == 0)
						sbQuery.Append(" AND K.�g�p�I���� = '"+ sData[4] + "' \n");
				}

				if(sData[6].Length > 0 && sData[7].Length > 0)
					sbQuery.Append(" AND SUBSTR(R.�o�^����,1,8)  BETWEEN '"+ sData[6] + "' AND '"+ sData[7] +"' \n");
				else
				{
					if(sData[6].Length > 0 && sData[7].Length == 0)
						sbQuery.Append(" AND SUBSTR(R.�o�^����,1,8) = '"+ sData[6] + "' \n");
				}
				sbQuery.Append(" ORDER BY R.����b�c,R.���p�҂b�c ");


				OracleDataReader reader;

				sbQuery2.Append(GET_KAIINCSV_SELECT);
				sbQuery2.Append(sbQuery);
				reader = CmdSelect(sUser, conn2, sbQuery2);

				StringBuilder sbData = new StringBuilder(1024);
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
					sbData.Append(sDbl + sSng + reader.GetString(0).Trim() + sDbl);				// ����b�c
					sbData.Append(sKanma + sDbl + reader.GetString(1).Trim() + sDbl);			// �����
					sbData.Append(sKanma + sDbl + reader.GetString(2).Trim() + sDbl);			// �g�p�J�n��
					sbData.Append(sKanma + sDbl + reader.GetString(3).Trim() + sDbl);			// �g�p�I����
					sbData.Append(sKanma + sDbl + reader.GetString(23).TrimEnd() + sDbl);		// �Ǘ��ҋ敪
					sbData.Append(sKanma + sDbl + reader.GetString(24).TrimEnd() + sDbl);		// �^����\���i�L���A�g�e�f�j
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(25).ToString().TrimEnd() + sDbl); // �o�^����
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(26).ToString().TrimEnd() + sDbl); // �X�V����
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(4).Trim() + sDbl);	// ����b�c
					sbData.Append(sKanma + sDbl + reader.GetString(5).Trim() + sDbl);			// ���喼
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(6).Trim() + sDbl);	// �Ǘ��X���b�c
					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);			// �Ǘ��X����
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).Trim() + sDbl);	// �ݒu��Z���P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).Trim() + sDbl);	// �ݒu��Z���Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(27).TrimEnd() + sDbl);		// Ver.�i�g�D�b�c�j
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(28).TrimEnd() + sDbl);		// �X�֔ԍ�
					sbData.Append(sKanma + sDbl + reader.GetDecimal(29).ToString().TrimEnd() + sDbl); // �g�p��
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(30).TrimEnd() + sDbl); // ����\���Ǘ��ԍ�
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(31).ToString().TrimEnd() + sDbl); // �o�^����
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(32).ToString().TrimEnd() + sDbl); // �X�V����
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).Trim() + sDbl);	// ���p�҂b�c
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).Trim() + sDbl);	// �p�X���[�h
					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl       );	// ���p�Җ�
					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);			// ���p�ғo�^��
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(33).TrimEnd() + sDbl); // �ב��l�b�c
					sbData.Append(sKanma + sDbl + reader.GetString(34).TrimEnd() + sDbl);		 // ���x������֎~
					sbData.Append(sKanma + sDbl + reader.GetDecimal(35).ToString().TrimEnd() + sDbl); // �F�؃G���[��
					sbData.Append(sKanma + sDbl + reader.GetString(36).TrimEnd() + sDbl); // �p�X���[�h�X�V���i�o�^�o�f�j
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(37).ToString().TrimEnd() + sDbl); // �o�^����
					sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(38).ToString().TrimEnd() + sDbl); // �X�V����
					sbData.Append(sKanma + sDbl + reader.GetDecimal(14) + sDbl);			// �T�[�}���䐔
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).Trim() + sDbl);	// �V���A���ԍ��P
					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);			// ��ԂP
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).Trim() + sDbl);	// �V���A���ԍ��Q
					sbData.Append(sKanma + sDbl + reader.GetString(18).Trim() + sDbl);			// ��ԂQ
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(19).Trim() + sDbl);	// �V���A���ԍ��R
					sbData.Append(sKanma + sDbl + reader.GetString(20).Trim() + sDbl);			// ��ԂR
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(21).Trim() + sDbl);	// �V���A���ԍ��S
					sbData.Append(sKanma + sDbl + reader.GetString(22).Trim() + sDbl);			// ��ԂS

					sList.Add(sbData);
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�擾
		 * �����F����b�c
		 * �ߒl�F�X�e�[�^�X�A����b�c�A������A�g�p�J�n���A�Ǘ��ҋ敪�A�g�p�I����
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(7634):
		*/
		[WebMethod]
		public string[] Sel_MemberTn(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "����}�X�^�����J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[8];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT CM01.����b�c "
					+       ",CM01.����� "
					+       ",CM01.�g�p�J�n�� "
					+       ",CM01.�Ǘ��ҋ敪 "
					+       ",CM01.�g�p�I���� "
					+       ",CM01.�X�V���� \n"
					+       ",CM01.�L���A�g�e�f \n"
					+  " FROM �b�l�O�P��� CM01\n"
					+  "     ,�b�l�O�Q���� CM02\n"
					+  "     ,�b�l�P�S�X�֔ԍ��i CM14\n" // ���q�^���Ή�
					+ " WHERE CM01.����b�c = '" + sKey[0] + "' \n"
					+    "AND CM01.�폜�e�f = '0' \n"
					+    "AND CM01.����b�c = CM02.����b�c \n"
					+    "AND CM02.�폜�e�f = '0' \n"
					+    "AND CM14.�X�֔ԍ� = CM02.�X�֔ԍ� \n"
					+    "AND CM14.�X���b�c = '" + sKey[1] + "' \n";

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
					sRet[0] = "�Y���f�[�^������܂���";
				else
					sRet[0] = "����I��";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�擾
		 * �����F����b�c
		 * �ߒl�F�X�e�[�^�X�A����b�c�A������A�g�p�J�n���A�Ǘ��ҋ敪�A�g�p�I����
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(127):
		*/
		[WebMethod]
		public string[] Sel_Member(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "����}�X�^�����J�n");

			OracleConnection conn2 = null;
			// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
			//			string[] sRet = new string[8];
			string[] sRet = new string[9];
			// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT ����b�c "
					+       ",����� "
					+       ",�g�p�J�n�� "
					+       ",�Ǘ��ҋ敪 "
					+       ",�g�p�I���� "
					+       ",�X�V���� \n"
					+       ",�L���A�g�e�f \n"
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					+       ",�ۗ�����e�f \n"
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					+  " FROM �b�l�O�P��� \n"
					// MOD 2011.06.01 ���s�j���� �r�p�k�̒��� START
					//					+ " WHERE ����b�c = '" + sKey[0] + "' \n"
					//					+ " OR ����b�c = 'J" + sKey[0] + "' \n" // ���q�^���Ή�
					//					+    "AND �폜�e�f = '0' \n"
					+ " WHERE ( ����b�c = '" + sKey[0] + "' \n"
					+ "  OR ����b�c = 'J" + sKey[0] + "' ) \n" // ���q�^���Ή�
					+ " AND �폜�e�f = '0' \n"
					+ " ORDER BY ����b�c \n"
					;
				// MOD 2011.06.01 ���s�j���� �r�p�k�̒��� END

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
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					sRet[8] = reader.GetString(7);
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if(iCnt == 1) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
					sRet[0] = "����I��";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�ꗗ�擾�Q
		 * �����F����b�c�A�����
		 * �ߒl�F�X�e�[�^�X�A����b�c�A�����
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(7741):
		*/
		[WebMethod]
		public string[] Get_MemberTn(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "����}�X�^�ꗗ�擾�Q�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT ���.������ from ( "
					+ "SELECT '|' "
					+     "|| TRIM(CM01.����b�c) || '|' "
					+     "|| TRIM(CM01.�����) || '|' "
					+     "|| TRIM(�g�p�I����) || '|' "
					+     "|| TO_CHAR(SYSDATE,'YYYYMMDD') || '|' "
					+     "������ \n"
					+  " FROM �b�l�O�P��� CM01\n"
					+  "     ,�b�l�O�Q���� CM02 \n"
					+  "     ,�b�l�P�S�X�֔ԍ��i CM14 \n"; // ���q�^���Ή�
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.����b�c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.����b�c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.����� LIKE '%" + sKey[1] + "%' \n";
				}

				cmdQuery += " AND CM01.�Ǘ��ҋ敪 IN ('1','3','4') \n"; // 1:�Ǘ��� 3:���q��� 4:���q�c�Ə�
				cmdQuery += " AND CM01.�폜�e�f = '0' \n";

				cmdQuery += " AND CM01.����b�c = CM02.����b�c \n";
				cmdQuery += " AND CM02.�폜�e�f = '0' \n";
				cmdQuery += " AND CM14.�X�֔ԍ� = CM02.�X�֔ԍ� \n";
				if (sKey[2].Trim().Length != 0)
				{
					cmdQuery += " AND CM14.�X���b�c = '" + sKey[2] + "' \n";
				}
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.�g�p�I���� >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += " ORDER BY CM01.����b�c \n";
				cmdQuery += " ) ��� GROUP BY ���.������ \n";

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
					sRet[0] = "�Y���f�[�^������܂���";
				}
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�ꗗ�擾�R
		 * �����F����b�c�A�����
		 * �ߒl�F�X�e�[�^�X�A����b�c�A�����
		 *
		 * �Q�ƌ��F��������Q.cs
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(7881):
		*/
		[WebMethod]
		public string[] Get_MemberTn3(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "����}�X�^�ꗗ�擾�R�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' "
					+     "|| TRIM(CM01.����b�c) || '|' "
					+     "|| TRIM(CM01.�����) || '|' "
					+     "|| TRIM(�g�p�I����) || '|' "
					+     "|| TO_CHAR(SYSDATE,'YYYYMMDD') || '|' "
					+     "������ \n"
					+     ", CM01.����b�c kcd \n"
					+  " FROM �b�l�O�P��� CM01\n";
				cmdQuery += "     ,�b�l�O�Q���� CM02 \n";
				cmdQuery += "     ,�b�l�P�S�X�֔ԍ��i CM14 \n"; // ���q�^���Ή�
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.����b�c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.����b�c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.����� LIKE '%" + sKey[1] + "%' \n";
				}
				cmdQuery += " AND CM01.�Ǘ��ҋ敪 IN ('1','3','4') \n"; // 1:�Ǘ��� 3:���q��� 4:���q�c�Ə�
				cmdQuery += " AND CM01.�폜�e�f = '0' \n";

				cmdQuery += " AND CM01.����b�c = CM02.����b�c \n"
					+ " AND CM02.�폜�e�f = '0' \n"
					+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
					;
				if (sKey[2].Trim().Length != 0)
					cmdQuery += " AND CM14.�X���b�c = '" + sKey[2] + "' \n";
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.�g�p�I���� >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					}
				}
				cmdQuery += "UNION \n";
				cmdQuery += "SELECT '|' "
					+ "|| TRIM(CM01.����b�c) || '|' "
					+ "|| TRIM(CM01.�����) || '|' ������ \n"
					+ ", CM01.����b�c \n"
					+ " FROM �b�l�O�P��� CM01 \n"
					+ "     ,�b�l�O�T������X CM05 \n";
				if (sKey[0].Trim().Length == 12)
				{
					cmdQuery += " WHERE CM01.����b�c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " WHERE CM01.����b�c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Trim().Length != 0)
				{
					cmdQuery += " AND CM01.����� LIKE '%" + sKey[1] + "%' \n";
				}
				cmdQuery += " AND CM01.�Ǘ��ҋ敪 IN ('1','3','4') \n"; // 3:���q��� 4:���q�c�Ə�
				cmdQuery += " AND CM01.�폜�e�f = '0' \n"
					+ " AND CM01.����b�c = CM05.����b�c \n"
					+ " AND CM05.�폜�e�f = '0' \n";
				if (sKey[2].Trim().Length != 0)
					cmdQuery += " AND CM05.�X���b�c = '" + sKey[2] + "' \n";
				if(sKey.Length >= 4)
				{
					if(sKey[3] == "1")
					{
						cmdQuery += " AND CM01.�g�p�I���� >= TO_CHAR(SYSDATE,'YYYYMMDD') \n";
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
					sRet[0] = "�Y���f�[�^������܂���";
				}
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ���˗���ꗗ�擾�iglobal�Ή��j
		 * �����F����b�c�A�ב��l���A�ב��l�b�c�A�X���b�c
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i���O�P�A�Z���P�A�ב��l�b�c�j...
		 *
		 * �Q�ƌ��F���˗��匟���Q.cs
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(8633):
		*/
		[WebMethod]
		public string[] Get_Goirainusi2(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "���˗���ꗗ�擾�Q�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' "
					+     "|| TRIM(SM01.����b�c) || '|' "
					+     "|| TRIM(CM01.�����) || '|' "
					+     "|| TRIM(CM02.���喼) || '|' "
					+     "|| TRIM(SM01.�ב��l�b�c) || '|' "
					+     "|| TRIM(SM01.���O�P) || '|' "
					+     "|| TRIM(SM01.�Z���P) || '|' "
					+     "|| TRIM(SM01.����b�c) || '|' \n"
					+    ",CM01.����b�c kcd \n"
					+  " FROM �r�l�O�P�ב��l SM01"
					+       ",�b�l�O�Q���� CM02"
					+       ",�b�l�P�S�X�֔ԍ��i CM14" // ���q�^���Ή�
					+       ",�b�l�O�P��� CM01 \n"
					+ " WHERE SM01.����b�c =  CM01.����b�c \n";
				if (sKey[0].Length == 10)
				{
					cmdQuery += " AND SM01.����b�c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " AND SM01.����b�c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Length == 12)
				{
					cmdQuery += " AND SM01.�ב��l�b�c = '" + sKey[1] + "' \n";
				}
				else
				{
					if (sKey[1].Length != 0)
					{
						cmdQuery += " AND SM01.�ב��l�b�c LIKE '" + sKey[1] + "%' \n";
					}
				}
				if (sKey[2].Length != 0)
				{
					cmdQuery += " AND SM01.���O�P LIKE '%" + sKey[2] + "%' \n";
				}
				cmdQuery += " AND SM01.����b�c =  CM02.����b�c \n"
					+  " AND SM01.����b�c =  CM02.����b�c \n"
					+  " AND CM02.�X�֔ԍ� =  CM14.�X�֔ԍ� \n"
					;
				if (sKey[3].Length != 0)
				{
					cmdQuery += " AND CM14.�X���b�c =  '" + sKey[3] + "' \n";
				}
				cmdQuery += " AND SM01.�폜�e�f = '0' \n"
					+  " AND CM02.�폜�e�f = '0' \n"
					+  " AND CM01.�폜�e�f = '0' \n";

				cmdQuery += "UNION \n";
				cmdQuery += "SELECT '|' "
					+     "|| TRIM(SM01.����b�c) || '|' "
					+     "|| TRIM(CM01.�����) || '|' "
					+     "|| TRIM(CM02.���喼) || '|' "
					+     "|| TRIM(SM01.�ב��l�b�c) || '|' "
					+     "|| TRIM(SM01.���O�P) || '|' "
					+     "|| TRIM(SM01.�Z���P) || '|' "
					+     "|| TRIM(SM01.����b�c) || '|' \n"
					+    ",CM01.����b�c \n"
					+  " FROM �r�l�O�P�ב��l SM01"
					+       ",�b�l�O�Q���� CM02"
					+       ",�b�l�O�T������X CM05"
					+       ",�b�l�O�P��� CM01 \n"
					+ " WHERE SM01.����b�c =  CM01.����b�c \n"
					+ "";
				if (sKey[0].Length == 10)
				{
					cmdQuery += " AND SM01.����b�c = '" + sKey[0] + "' \n";
				}
				else
				{
					cmdQuery += " AND SM01.����b�c LIKE '" + sKey[0] + "%' \n";
				}
				if (sKey[1].Length == 12)
				{
					cmdQuery += " AND SM01.�ב��l�b�c = '" + sKey[1] + "' \n";
				}
				else
				{
					if (sKey[1].Length != 0)
					{
						cmdQuery += " AND SM01.�ב��l�b�c LIKE '" + sKey[1] + "%' \n";
					}
				}
				if (sKey[2].Length != 0)
				{
					cmdQuery += " AND SM01.���O�P LIKE '%" + sKey[2] + "%' \n";
				}
				cmdQuery += " AND SM01.����b�c =  CM02.����b�c \n"
					+  " AND SM01.����b�c =  CM02.����b�c \n"
					+  " AND SM01.����b�c =  CM05.����b�c \n"
					;
				if (sKey[3].Length != 0)
				{
					cmdQuery += " AND CM05.�X���b�c =  '" + sKey[3] + "' \n";
				}
				cmdQuery += " AND SM01.�폜�e�f = '0' \n"
					+  " AND CM02.�폜�e�f = '0' \n"
					+  " AND CM05.�폜�e�f = '0' \n"
					+  " AND CM01.�폜�e�f = '0' \n";
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
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �˗���f�[�^�擾
		 * �����F����b�c�A����b�c�A�ב��l�b�c�A�X���b�c
		 * �ߒl�F�X�e�[�^�X�A�J�i���́A�d�b�ԍ��A�X�֔ԍ��A�Z���A���O�A�d��
		 *		 ���[���A�h���X�A���Ӑ�b�c�A���Ӑ敔�ۂb�c�A�X�V����
		 *********************************************************************/
		private static string GET_SIRAINUSI_SELECT1
			= "SELECT SM01.���O�P \n"
			+ " FROM �r�l�O�P�ב��l SM01 \n"
			+ ", �b�l�O�Q���� CM02 \n"
			+ ", �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
			+ "";

		private static string GET_SIRAINUSI_SELECT2
			= "SELECT CM02.���喼 \n"
			+ " FROM �b�l�O�Q���� CM02 \n"
			+ ", �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
			+ "";

		private static string GET_SIRAINUSI_SELECT3
			= "SELECT CM01.����� \n"
			+ " FROM �b�l�O�P��� CM01 \n"
			+ ", �b�l�O�Q���� CM02 \n"
			+ ", �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
			+ "";
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(10314):
		*/
		/*
				[WebMethod]
				public String[] Get_Sirainusi(string[] sUser, string sKCode, string sBCode, string sICode, string sTCode)
				{
					logWriter(sUser, INF, "�˗�����擾�J�n");

					OracleConnection conn2 = null;
					string[] sRet = new string[4]{"","","",""};

					// �c�a�ڑ�
					conn2 = connect2(sUser);
					if(conn2 == null)
					{
						sRet[0] = "�c�a�ڑ��G���[";
						return sRet;
					}
					try
					{
						string cmdQuery = "";
						OracleDataReader reader;

						if(sKCode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT3
								+ " WHERE CM01.����b�c = '" + sKCode + "' \n"
								+ " AND CM01.�폜�e�f = '0' \n"
								+ " AND CM01.����b�c = CM02.����b�c \n"
								+ " AND CM02.�폜�e�f = '0' \n"
								+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
								+ "";

							//�X���b�c���ݒ肳��Ă��鎞
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[1]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
							if(sBCode.Length > 0)
							{
								cmdQuery = GET_SIRAINUSI_SELECT2
									+ " WHERE CM02.����b�c = '" + sKCode + "' \n"
									+ " AND CM02.����b�c = '" + sBCode + "' \n"
									+ " AND CM02.�폜�e�f = '0' \n"
									+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
									+ "";

								//�X���b�c���ݒ肳��Ă��鎞
								if(sTCode.Length > 0)
								{
									cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
								}

								reader = CmdSelect(sUser, conn2, cmdQuery);

								if(reader.Read()) sRet[2]  = reader.GetString(0).Trim();
								disposeReader(reader);
								reader = null;

								if(sICode.Length > 0)
								{
									cmdQuery = GET_SIRAINUSI_SELECT1
										+ " WHERE SM01.����b�c = '" + sKCode + "' \n"
										+ " AND SM01.����b�c = '" + sBCode + "' \n"
										+ " AND SM01.�ב��l�b�c = '" + sICode + "' \n"
										+ " AND SM01.�폜�e�f = '0' \n"
										+ " AND SM01.����b�c = CM02.����b�c \n"
										+ " AND SM01.����b�c = CM02.����b�c \n"
										+ " AND CM02.�폜�e�f = '0' \n"
										+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
										+ "";

									//�X���b�c���ݒ肳��Ă��鎞
									if(sTCode.Length > 0)
									{
										cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
									}

									reader = CmdSelect(sUser, conn2, cmdQuery);

									if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
									disposeReader(reader);
									reader = null;
								}
							}
							else
							{
								//����b�c�������͂̏ꍇ
								if(sICode.Length > 0)
								{
									cmdQuery = GET_SIRAINUSI_SELECT1
										+ " WHERE SM01.����b�c = '" + sKCode + "' \n"
										+ " AND SM01.�ב��l�b�c = '" + sICode + "' \n"
										+ " AND SM01.�폜�e�f = '0' \n"
										+ " AND SM01.����b�c = CM02.����b�c \n"
										+ " AND SM01.����b�c = CM02.����b�c \n"
										+ " AND CM02.�폜�e�f = '0' \n"
										+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
										+ "";

									//�X���b�c���ݒ肳��Ă��鎞
									if(sTCode.Length > 0)
									{
										cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
									}

									reader = CmdSelect(sUser, conn2, cmdQuery);

									if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
									disposeReader(reader);
									reader = null;
								}
							}
						}

						sRet[0] = "����I��";
						logWriter(sUser, INF, sRet[0]);
					}
					catch (OracleException ex)
					{
						sRet[0] = chgDBErrMsg(sUser, ex);
					}
					catch (Exception ex)
					{
						sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �˗�����擾�Q
		 * �����F���[�U�[�A����b�c�A����b�c�A�ב��l�b�c�A�X���b�c
		 * �ߒl�F�˗�����
		 *
		 * �Q�ƌ��F�o�׏Ɖ�.cs
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(10479):
		*/
		[WebMethod]
		public String[] Get_Sirainusi2(string[] sUser, string sKCode, string sBCode, string sICode, string sTCode)
		{
			logWriter(sUser, INF, "�˗�����擾�Q�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[4]{"","","",""};

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			try
			{
				string cmdQuery = "";
				OracleDataReader reader;

				if(sKCode.Length > 0)
				{
					cmdQuery = GET_SIRAINUSI_SELECT3
						+ " WHERE CM01.����b�c = '" + sKCode + "' \n"
						+ " AND CM01.�폜�e�f = '0' \n"
						+ " AND CM01.����b�c = CM02.����b�c \n"
						+ " AND CM02.�폜�e�f = '0' \n"
						+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
						+ "";

					//�X���b�c���ݒ肳��Ă��鎞
					if(sTCode.Length > 0)
					{
						cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
					}
					//�X���b�c���ݒ肳��Ă��鎞
					if (sTCode.Length > 0) 
					{
						cmdQuery += "UNION \n";
						cmdQuery += "SELECT CM01.����� \n"
							+ " FROM �b�l�O�P��� CM01 \n"
							+ "     ,�b�l�O�T������X CM05 \n"
							+ " WHERE CM01.����b�c = '" + sKCode + "' \n"
							+ " AND CM01.�폜�e�f = '0' \n"
							+ " AND CM01.����b�c = CM05.����b�c \n"
							+ " AND CM05.�폜�e�f = '0' \n"
							+ " AND CM05.�X���b�c = '" + sTCode + "' \n";
					}

					reader = CmdSelect(sUser, conn2, cmdQuery);

					if(reader.Read()) sRet[1]  = reader.GetString(0).Trim();
					disposeReader(reader);
					reader = null;

					if(sBCode.Length > 0)
					{
						cmdQuery = GET_SIRAINUSI_SELECT2
							+ " WHERE CM02.����b�c = '" + sKCode + "' \n"
							+ " AND CM02.����b�c = '" + sBCode + "' \n"
							+ " AND CM02.�폜�e�f = '0' \n"
							+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
							+ "";

						//�X���b�c���ݒ肳��Ă��鎞
						if(sTCode.Length > 0)
						{
							cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
						}

						//�X���b�c���ݒ肳��Ă��鎞
						if (sTCode.Length > 0) 
						{
							cmdQuery += "UNION \n";
							cmdQuery += "SELECT CM02.���喼 \n"
								+ " FROM �b�l�O�Q���� CM02 \n"
								+ "     ,�b�l�O�T������X CM05 \n"
								+ " WHERE CM02.����b�c = '" + sKCode + "' \n"
								+ " AND CM02.����b�c = '" + sBCode + "' \n"
								+ " AND CM02.�폜�e�f = '0' \n"
								+ " AND CM02.����b�c = CM05.����b�c \n"
								+ " AND CM05.�폜�e�f = '0' \n"
								+ " AND CM05.�X���b�c = '" + sTCode + "' \n";
						}

						reader = CmdSelect(sUser, conn2, cmdQuery);

						if(reader.Read()) sRet[2]  = reader.GetString(0).Trim();
						disposeReader(reader);
						reader = null;

						if(sICode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT1
								+ " WHERE SM01.����b�c = '" + sKCode + "' \n"
								+ " AND SM01.����b�c = '" + sBCode + "' \n"
								+ " AND SM01.�ב��l�b�c = '" + sICode + "' \n"
								+ " AND SM01.�폜�e�f = '0' \n"
								+ " AND SM01.����b�c = CM02.����b�c \n"
								+ " AND SM01.����b�c = CM02.����b�c \n"
								+ " AND CM02.�폜�e�f = '0' \n"
								+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
								+ "";

							//�X���b�c���ݒ肳��Ă��鎞
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
							}

							//�X���b�c���ݒ肳��Ă��鎞
							if (sTCode.Length > 0) 
							{
								cmdQuery += "UNION \n";
								cmdQuery += "SELECT SM01.���O�P \n"
									+ " FROM �r�l�O�P�ב��l SM01 \n"
									+ "     ,�b�l�O�T������X CM05 \n"
									+ " WHERE SM01.����b�c = '" + sKCode + "' \n"
									+ " AND SM01.����b�c = '" + sBCode + "' \n"
									+ " AND SM01.�ב��l�b�c = '" + sICode + "' \n"
									+ " AND SM01.�폜�e�f = '0' \n"
									+ " AND SM01.����b�c = CM05.����b�c \n"
									+ " AND CM05.�폜�e�f = '0' \n"
									+ " AND CM05.�X���b�c = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
						}
					}
					else
					{
						//����b�c�������͂̏ꍇ
						if(sICode.Length > 0)
						{
							cmdQuery = GET_SIRAINUSI_SELECT1
								+ " WHERE SM01.����b�c = '" + sKCode + "' \n"
								+ " AND SM01.�ב��l�b�c = '" + sICode + "' \n"
								+ " AND SM01.�폜�e�f = '0' \n"
								+ " AND SM01.����b�c = CM02.����b�c \n"
								+ " AND SM01.����b�c = CM02.����b�c \n"
								+ " AND CM02.�폜�e�f = '0' \n"
								+ " AND CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n"
								+ "";

							//�X���b�c���ݒ肳��Ă��鎞
							if(sTCode.Length > 0)
							{
								cmdQuery += " AND CM14.�X���b�c = '" + sTCode + "' \n";
							}

							//�X���b�c���ݒ肳��Ă��鎞
							if (sTCode.Length > 0) 
							{
								cmdQuery += "UNION \n";
								cmdQuery += "SELECT SM01.���O�P \n"
									+ " FROM �r�l�O�P�ב��l SM01 \n"
									+ "     ,�b�l�O�T������X CM05 \n"
									+ " WHERE SM01.����b�c = '" + sKCode + "' \n"
									+ " AND SM01.�ב��l�b�c = '" + sICode + "' \n"
									+ " AND SM01.�폜�e�f = '0' \n"
									+ " AND SM01.����b�c = CM05.����b�c \n"
									+ " AND CM05.�폜�e�f = '0' \n"
									+ " AND CM05.�X���b�c = '" + sTCode + "' \n";
							}

							reader = CmdSelect(sUser, conn2, cmdQuery);

							if(reader.Read()) sRet[3]  = reader.GetString(0).Trim();
							disposeReader(reader);
							reader = null;
						}
					}
				}

				sRet[0] = "����I��";
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �o�׈���f�[�^�擾
		 * �����F����b�c�A����b�c�A�o�^���A�W���[�i���m�n
		 * �ߒl�F�X�e�[�^�X�A�׎�l�b�c�A�d�b�ԍ��A�Z��...
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2print\Service1.asmx.cs(101):
		*/
		[WebMethod]
		public String[] Get_InvoicePrintData(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "�o�׈���f�[�^�擾�J�n");

			OracleConnection conn2 = null;
			// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� START
			//			string[] sRet = new string[40];
			// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
			//			string[] sRet = new string[41];
			// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
			//			string[] sRet = new string[42];
			// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ START
			//			string[] sRet = new string[45];
			string[] sRet = new string[46];
			// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ END
			// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
			// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
			// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� END
			// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ START
			string s���p�ҕ���X���b�c = (sKey.Length >  4) ? sKey[ 4] : "";
			// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ END
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			decimal d�ː� = 0;
			string s�X�֔ԍ� = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT ");
				sbQuery.Append(" ST01.�׎�l�b�c ");
				sbQuery.Append(",ST01.�d�b�ԍ��P ");
				sbQuery.Append(",ST01.�d�b�ԍ��Q ");
				sbQuery.Append(",ST01.�d�b�ԍ��R ");
				sbQuery.Append(",ST01.�Z���P ");
				sbQuery.Append(",ST01.�Z���Q ");
				sbQuery.Append(",ST01.�Z���R ");
				sbQuery.Append(",ST01.���O�P ");
				sbQuery.Append(",ST01.���O�Q ");
				sbQuery.Append(",ST01.�o�ד� ");
				sbQuery.Append(",ST01.�����ԍ� ");
				sbQuery.Append(",ST01.�X�֔ԍ� ");
				sbQuery.Append(",ST01.���X�b�c ");
				sbQuery.Append(",NVL(CM14.�X���b�c, ST01.���X�b�c)");
				sbQuery.Append(",SM01.�d�b�ԍ��P ");
				sbQuery.Append(",SM01.�d�b�ԍ��Q ");
				sbQuery.Append(",SM01.�d�b�ԍ��R ");
				sbQuery.Append(",SM01.�Z���P ");
				sbQuery.Append(",SM01.�Z���Q ");
				sbQuery.Append(",SM01.�Z���R ");
				sbQuery.Append(",SM01.���O�P ");
				sbQuery.Append(",SM01.���O�Q ");
				sbQuery.Append(",ST01.�� ");
				sbQuery.Append(",ST01.�d�� ");
				sbQuery.Append(",ST01.�ی����z ");
				sbQuery.Append(",ST01.�w��� ");
				sbQuery.Append(",ST01.�A���w���P ");
				sbQuery.Append(",ST01.�A���w���Q ");
				sbQuery.Append(",ST01.�i���L���P ");
				sbQuery.Append(",ST01.�i���L���Q ");
				sbQuery.Append(",ST01.�i���L���R ");
				sbQuery.Append(",ST01.�����敪 ");
				sbQuery.Append(",ST01.����󔭍s�ςe�f ");
				sbQuery.Append(",ST01.�ː� \n");
				sbQuery.Append(",ST01.�ב��l������ ");
				sbQuery.Append(",ST01.���q�l�o�הԍ� ");
				sbQuery.Append(",ST01.�A���w���b�c�P ");
				sbQuery.Append(",ST01.�A���w���b�c�Q ");
				sbQuery.Append(",ST01.�w����敪 ");
				sbQuery.Append(",ST01.�X�֔ԍ� ");
				sbQuery.Append(",ST01.�d���b�c ");
				sbQuery.Append(",NVL(CM10.�X����, ST01.���X��)");
				sbQuery.Append(",ST01.�o�׍ςe�f ");
				// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� START
				sbQuery.Append(",SM01.�X�֔ԍ� ");
				// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� END
				// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				sbQuery.Append(",NVL(CM01.�ۗ�����e�f,'0') \n");
				// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
				sbQuery.Append(",ST01.�i���L���S ,ST01.�i���L���T ,ST01.�i���L���U \n");
				// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
				// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ START
				sbQuery.Append(",ST01.���X�� ");
				// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ END
				sbQuery.Append(" FROM \"�r�s�O�P�o�׃W���[�i��\" ST01");
				sbQuery.Append("\n");
				sbQuery.Append(" LEFT JOIN �b�l�O�Q���� CM02 \n");
				sbQuery.Append(" ON ST01.����b�c = CM02.����b�c \n");
				sbQuery.Append("AND ST01.����b�c = CM02.����b�c \n");
				sbQuery.Append(" LEFT JOIN �b�l�P�S�X�֔ԍ��i CM14 \n"); // ���q�^���Ή�
				sbQuery.Append(" ON CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n");
				sbQuery.Append(" LEFT JOIN �b�l�P�O�X�� CM10 \n");
				sbQuery.Append(" ON CM14.�X���b�c = CM10.�X���b�c \n");
				// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				sbQuery.Append(" LEFT JOIN �b�l�O�P��� CM01 \n");
				sbQuery.Append(" ON ST01.����b�c = CM01.����b�c \n");
				// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				sbQuery.Append(", \"�r�l�O�P�ב��l\" SM01 \n");
				sbQuery.Append(" WHERE ST01.����b�c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND ST01.����b�c = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND ST01.�o�^�� = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND ST01.�W���[�i���m�n = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND ST01.����b�c = SM01.����b�c \n");
				sbQuery.Append(" AND ST01.����b�c = SM01.����b�c \n");
				sbQuery.Append(" AND ST01.�ב��l�b�c = SM01.�ב��l�b�c \n");
				sbQuery.Append(" AND ST01.�폜�e�f = '0' \n");
				sbQuery.Append(" AND SM01.�폜�e�f = '0' \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				int iCnt = 0;
				if (reader.Read())
				{
					string s�A�����i�b�c�P = reader.GetString(36).Trim();
					string s�A�����i�b�c�Q = reader.GetString(37).Trim();
					sRet[1]  = reader.GetString(0).Trim();
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
					//					sRet[5]  = reader.GetString(4).Trim();
					//					sRet[6]  = reader.GetString(5).Trim();
					//					sRet[7]  = reader.GetString(6).Trim();
					//					sRet[8]  = reader.GetString(7).Trim();
					//					sRet[9]  = reader.GetString(8).Trim();
					sRet[5]  = reader.GetString(4).TrimEnd(); // �׎�l�Z���P
					sRet[6]  = reader.GetString(5).TrimEnd(); // �׎�l�Z���Q
					sRet[7]  = reader.GetString(6).TrimEnd(); // �׎�l�Z���R
					sRet[8]  = reader.GetString(7).TrimEnd(); // �׎�l���O�P
					sRet[9]  = reader.GetString(8).TrimEnd(); // �׎�l���O�Q
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[10] = reader.GetString(9).Trim();
					sRet[11] = reader.GetString(10).Trim();
					sRet[12] = reader.GetString(11).Trim();
					sRet[13] = reader.GetString(12).Trim().PadLeft(4, '0');
					sRet[14] = reader.GetString(13).Trim().PadLeft(4, '0');
					sRet[15] = reader.GetString(14).Trim();
					sRet[16] = reader.GetString(15).Trim();
					sRet[17] = reader.GetString(16).Trim();
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
					//					sRet[18] = reader.GetString(17).Trim();
					//					sRet[19] = reader.GetString(18).Trim();
					//					sRet[20] = reader.GetString(19).Trim();
					//					sRet[21] = reader.GetString(20).Trim();
					//					sRet[22] = reader.GetString(21).Trim();
					sRet[18] = reader.GetString(17).TrimEnd(); // �ב��l�Z���P
					sRet[19] = reader.GetString(18).TrimEnd(); // �ב��l�Z���Q
					sRet[20] = reader.GetString(19).TrimEnd(); // �ב��l�Z���R
					sRet[21] = reader.GetString(20).TrimEnd(); // �ב��l���O�P
					sRet[22] = reader.GetString(21).TrimEnd(); // �ב��l���O�Q
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[23] = reader.GetDecimal(22).ToString().Trim();
					// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� START
					//					d�ː�    = reader.GetDecimal(33);
					//					d�ː�    = d�ː� * 8;
					//					if(d�ː� == 0)
					//						sRet[24] = reader.GetDecimal(23).ToString().Trim();
					//					else
					//						sRet[24] = d�ː�.ToString().Trim();
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					if(reader.GetString(44) == "1")
					{
						d�ː� = reader.GetDecimal(33) * 8;
						if(d�ː� == 0)
						{
							sRet[24] = reader.GetDecimal(23).ToString().TrimEnd();
						}
						else
						{
							sRet[24] = d�ː�.ToString().TrimEnd();
						}
					}
					else
					{
						// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						sRet[24] = "";
						// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					}
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� END
					sRet[25] = reader.GetDecimal(24).ToString().Trim();
					sRet[26] = reader.GetString(25).Trim();
					if (s�A�����i�b�c�P.Equals("100"))
					{
						sRet[27] = reader.GetString(27).TrimEnd();
						sRet[28] = "";
					}
						// �P�s�ڂƂQ�s�ڂ������R�[�h�̏ꍇ�A�Q�s�ڂ�\�����Ȃ�
					else if (s�A�����i�b�c�P.Equals(s�A�����i�b�c�Q))
					{
						sRet[27] = reader.GetString(26).TrimEnd();
						sRet[28] = "";
					}
					else
					{
						sRet[27] = reader.GetString(26).TrimEnd();
						sRet[28] = reader.GetString(27).TrimEnd();
					}
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
					//					sRet[29] = reader.GetString(28).Trim();
					//					sRet[30] = reader.GetString(29).Trim();
					//					sRet[31] = reader.GetString(30).Trim();
					sRet[29] = reader.GetString(28).TrimEnd(); // �i���L���P
					sRet[30] = reader.GetString(29).TrimEnd(); // �i���L���Q
					sRet[31] = reader.GetString(30).TrimEnd(); // �i���L���R
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					// �p�[�Z���̏ꍇ�A"11"
					if (s�A�����i�b�c�P.Equals("001") || s�A�����i�b�c�P.Equals("002"))
						sRet[32] = reader.GetString(31).Trim() + "1";
					else
						sRet[32] = reader.GetString(31).Trim() + "0";
					sRet[33] = reader.GetString(32).Trim();
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
					//					sRet[34] = reader.GetString(34).Trim();
					sRet[34] = reader.GetString(34).TrimEnd(); // �S���ҁi�����j
					// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[35] = reader.GetString(35).Trim(); // ���q�l�ԍ�
					sRet[36] = reader.GetString(38).Trim();
					s�X�֔ԍ� = reader.GetString(39).Trim();
					sRet[37] = reader.GetString(40).Trim();		//�d���b�c
					sRet[38] = reader.GetString(41).Trim();		//���X��
					sRet[39] = reader.GetString(42).Trim();		//�o�׍ςe�f
					// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� START
					sRet[40] = reader.GetString(43).Trim();		//���˗���X�֔ԍ�
					// MOD 2011.01.06 ���s�j���� �X�֔ԍ��̈�� END
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					sRet[41] = reader.GetString(44).TrimEnd();
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
					sRet[42] = reader.GetString(45).TrimEnd(); // �i���L���S
					sRet[43] = reader.GetString(46).TrimEnd(); // �i���L���T
					sRet[44] = reader.GetString(47).TrimEnd(); // �i���L���U
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
					// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ START
					sRet[45] = reader.GetString(48).TrimEnd(); // ���X��
					// MOD 2011.12.06 ���s�j���� ���x���w�b�_���ɔ��X���E���X������ END
					iCnt++;
				}
				disposeReader(reader);
				reader = null;
				if (iCnt == 0)
				{
					sRet[0] = "�Y���f�[�^������܂���";
				}
				else
				{
					sRet[0] = "����I��";
					// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ START
					if(s���p�ҕ���X���b�c.Length == 0)
					{
						// MOD 2011.10.06 ���s�j���� �o�׃f�[�^�̈�����O�̒ǉ� START
						logWriter(sUser, INF, "�o�׈���f�[�^�擾�@���p�ҕ���X���b�c��"
							+"["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sRet[11]+"]"
							+"����󔭍s��["+sRet[33]+"]�o�׍�["+sRet[39]+"]"
							);
						// MOD 2011.10.06 ���s�j���� �o�׃f�[�^�̈�����O�̒ǉ� END
						return sRet;
					}
					// ���p�҂̕���̊Ǌ��X���b�c�Ɠo�^�҂̔��X�b�c�Ƃ��قȂ�ꍇ
					string s���X�b�c = sRet[14].Trim().Substring(1, 3);
					if(!s���X�b�c.Equals(s���p�ҕ���X���b�c))
					{
						return sRet;
					}
					// �����ԍ����Ȃ��ꍇ�ɂ͎擾����
					if(sRet[11].Length == 0)
					{
						disconnect2(sUser, conn2);
						conn2 = null;

						string[] sRetInvoiceNo = Set_InvoiceNo2(sUser ,sKey, sRet, s���p�ҕ���X���b�c);
						if(sRetInvoiceNo[0].Length == 4)
						{
							//							sRet[11] = sRetInvoiceNo[1];
						}
						else
						{
							sRet[0] = sRetInvoiceNo[0];
						}
					}
					// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ END
					// MOD 2011.10.06 ���s�j���� �o�׃f�[�^�̈�����O�̒ǉ� START
					logWriter(sUser, INF, "�o�׈���f�[�^�擾"
						+"["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sRet[11]+"]"
						+"����󔭍s��["+sRet[33]+"]�o�׍�["+sRet[39]+"]"
						);
					// MOD 2011.10.06 ���s�j���� �o�׃f�[�^�̈�����O�̒ǉ� END
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����󔭍s�ςe�f�̍X�V
		 * �����F����b�c�A����b�c�A�o�^���A�W���[�i���m�n�A�����ԍ��A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2print\Service1.asmx.cs(778):
		*/
		[WebMethod]
		public String[] Set_InvoiceNo(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "���s�ςe�f�X�V�J�n");

			OracleConnection conn2 = null;
			// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ START
			//			string[] sRet = new string[1];
			string[] sRet = new string[2]{"",""};
			// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ END
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string s���X�b�c = "";
				string s���X��   = "";
				sbQuery.Append("SELECT NVL(CM14.�X���b�c, ' ') \n");
				sbQuery.Append(", NVL(CM10.�X����, ' ') \n");
				sbQuery.Append(" FROM �b�l�O�Q���� CM02 \n");
				sbQuery.Append(" LEFT JOIN �b�l�P�S�X�֔ԍ��i CM14 \n"); // ���q�^���Ή�
				sbQuery.Append(" ON CM02.�X�֔ԍ� = CM14.�X�֔ԍ� \n");
				sbQuery.Append(" LEFT JOIN �b�l�P�O�X�� CM10 \n");
				sbQuery.Append(" ON CM14.�X���b�c = CM10.�X���b�c \n");
				sbQuery.Append(" WHERE CM02.����b�c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND CM02.����b�c = '" + sKey[1] + "' \n");
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s���X�b�c = reader.GetString(0).Trim();
					s���X��   = reader.GetString(1).Trim();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ START
				// �����ԍ��`�F�b�N
				sbQuery = new StringBuilder(1024);
				string s�����ԍ� = "";
				sbQuery.Append("SELECT �����ԍ� \n");
				sbQuery.Append(" FROM  \"�r�s�O�P�o�׃W���[�i��\" \n");
				sbQuery.Append(" WHERE ����b�c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND ����b�c = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND �o�^��   = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND \"�W���[�i���m�n\" = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND �폜�e�f = '0' \n");
				sbQuery.Append(" FOR UPDATE \n");
				reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s�����ԍ� = reader.GetString(0).TrimEnd();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				if(s�����ԍ�.Length > 0)
				{
					// �قȂ鑗���ԍ����㏑�����悤�Ƃ����ꍇ
					if(s�����ԍ� != sKey[4])
					{
						tran.Commit();
						sRet[0] = "�G���[�F���̒[���ň�����������͈���ςł�\n"
							+ "["+s�����ԍ�.Substring(4)+"]";
						sRet[1] = s�����ԍ�;
						logWriter(sUser, INF, "�����ԍ��X�V��["+sRet[1]+"]"
							+ " ["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sKey[4]+"]");
						return sRet;
					}
				}

				// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ END
				// �o�׃W���[�i���̍X�V
				string cmdQuery  = "UPDATE \"�r�s�O�P�o�׃W���[�i��\" \n";
				cmdQuery += " SET �����ԍ� = '"  + sKey[4] + "' ";                     // �����ԍ�
				// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ START
				cmdQuery +=     ",�����O�P = TO_CHAR(SYSDATE,'MMDDHH24MISS') \n"; // ����������������b
				// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ END
				cmdQuery +=     ",����󔭍s�ςe�f = '1' ";
				cmdQuery +=     ",��� = DECODE(���,'01','02','02','02',���) ";
				cmdQuery +=     ",�ڍ׏�� = DECODE(���,'01','  ','02','  ',�ڍ׏��) ";
				cmdQuery +=     ",�X�V���� =   TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";    // �X�V����
				cmdQuery +=     ",�X�V�o�f = '�o�דo�^' ";                               // �X�V�o�f
				cmdQuery +=     ",�X�V�� = '" + sKey[5] + "' \n";                        // �X�V��
				if(s���X�b�c.Length > 0)
				{
					cmdQuery += ",���X�b�c = '" + s���X�b�c + "' \n";
				}
				if(s���X��.Length > 0)
				{
					cmdQuery += ",���X�� = '"   + s���X��   + "' \n";
				}
				cmdQuery += " WHERE ����b�c       = '" + sKey[0] + "' \n";
				cmdQuery +=   " AND ����b�c       = '" + sKey[1] + "' \n";
				cmdQuery +=   " AND �o�^��         = '" + sKey[2] + "' \n";
				cmdQuery +=   " AND �W���[�i���m�n = '" + sKey[3] + "' \n";
				cmdQuery +=   " AND �폜�e�f       = '0' \n";
				logWriter(sUser, INF, "���s�ςe�f�X�V["+sKey[1]+"]["+sKey[2]+"]["+sKey[3]+"]:["+sKey[4]+"]");

				CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "����I��";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
		// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ START
		/*********************************************************************
		 * �̔Ԃ̍X�V
		 * �����F����b�c�A����b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2print\Service1.asmx.cs(494):
		*/
		[WebMethod]
		public String[] Get_InvoiceNo(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "�̔ԍX�V�J�n");
			
			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			//�g�����U�N�V�����̐ݒ�
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				decimal i�o�^�A��     = 0;
				decimal i�J�n���[�ԍ� = 0;
				decimal i�I�����[�ԍ� = 0;
				decimal i�ŏI���[�ԍ� = 0;
				string  s���t��       = "";
				string  s�L������     = "";
				string  s�������t     = "";

				string cmdQuery_am12 = "SELECT";
				cmdQuery_am12 += " AM12.�o�^�A�� ";
				cmdQuery_am12 += ",AM12.�J�n���[�ԍ� ";
				cmdQuery_am12 += ",AM12.�I�����[�ԍ� ";
				cmdQuery_am12 += ",AM12.�ŏI���[�ԍ� ";
				cmdQuery_am12 += ",AM12.���t�� ";
				cmdQuery_am12 += ",AM12.�L������ ";
				cmdQuery_am12 += ",TO_CHAR(SYSDATE,'YYYYMMDD') \n";
				cmdQuery_am12 += " FROM �`�l�P�Q�����̔� AM12 \n";
				cmdQuery_am12 += " WHERE AM12.����b�c = '" + sKey[0] + "' \n";
				cmdQuery_am12 += " AND AM12.����b�c = '" + sKey[1] + "' \n";
				cmdQuery_am12 += " AND AM12.�����敪 = '" + sKey[2] + "' \n";
				cmdQuery_am12 += " AND AM12.�폜�e�f = '0' \n";
				cmdQuery_am12 += " FOR UPDATE \n";

				OracleDataReader reader_am12 = CmdSelect(sUser, conn2, cmdQuery_am12);
				int intCnt_am12 = 0;
				sRet[1] = "";
				if (reader_am12.Read())
				{
					i�o�^�A��     = reader_am12.GetDecimal(0);
					i�J�n���[�ԍ� = reader_am12.GetDecimal(1);
					i�I�����[�ԍ� = reader_am12.GetDecimal(2);
					i�ŏI���[�ԍ� = reader_am12.GetDecimal(3);
					s���t��       = reader_am12.GetString(4).Trim();
					s�L������     = reader_am12.GetString(5).Trim();
					s�������t     = reader_am12.GetString(6).Trim();
					intCnt_am12++;

					if (i�ŏI���[�ԍ� < i�I�����[�ԍ� && int.Parse(s�L������) >= int.Parse(s�������t))
					{
						//�����ԍ��̃Z�b�g
						sRet[1] = (i�ŏI���[�ԍ� + 1).ToString();
					}
				}
				disposeReader(reader_am12);
				reader_am12 = null;
				if (sRet[1].Length == 0)
				{
					//�`�l�P�Q�����̔ԂɃL�[�����݂��Ȃ��A�܂���
					//�ŏI�ԍ� >= �I���ԍ��A�܂���
					//�L������ <  �����̎�
					decimal i�ő�A��   = 0;
					decimal i�J�n�ԍ�   = 0;
					decimal i�ŏI�ԍ�   = 0;
					decimal i�I���ԍ�   = 0;
					decimal i���t����   = 0;
					decimal i�L������   = 0;
					decimal i�L�������N = 0;
					decimal i�L�������� = 0;
					decimal i�L�������� = 0;

					//�̔ԊǗ����V�K���[�ԍ��g���擾
					string cmdQuery_am10 = "SELECT";
					cmdQuery_am10 += " AM10.�ő�A�� ";
					cmdQuery_am10 += ",AM10.�o�^�A�� ";
					cmdQuery_am10 += ",AM10.�ŏI���[�ԍ� ";
					cmdQuery_am10 += ",AM11.�I�����[�ԍ� ";
					cmdQuery_am10 += ",AM10.���t���� ";
					cmdQuery_am10 += ",AM10.�L������ ";
					cmdQuery_am10 += ",TO_CHAR(SYSDATE,'YYYYMMDD') \n";
					cmdQuery_am10 += "FROM �`�l�P�O�̔ԊǗ� AM10 ";
					cmdQuery_am10 += ",�`�l�P�P�����ԍ� AM11 \n";
					cmdQuery_am10 += " WHERE AM10.�̔ԋ敪 = '" + sKey[2] + "' \n";
					//cmdQuery_am10 += "   AND AM10.�o�^�A��       =  " + i�o�^�A��;
					cmdQuery_am10 += " AND AM10.�̔ԋ敪 = AM11.�����敪 \n";
					cmdQuery_am10 += " AND AM10.�o�^�A�� = AM11.�o�^�A�� \n";
					cmdQuery_am10 += " AND AM10.�폜�e�f = '0' \n";
					cmdQuery_am10 += " FOR UPDATE \n";

					OracleDataReader reader_am10 = CmdSelect(sUser, conn2, cmdQuery_am10);
					int intCnt_am10 = 0;
					if (reader_am10.Read())
					{
						i�ő�A��     = reader_am10.GetDecimal(0);
						i�o�^�A��     = reader_am10.GetDecimal(1);
						i�ŏI�ԍ�     = reader_am10.GetDecimal(2);
						i�I���ԍ�     = reader_am10.GetDecimal(3);
						i���t����     = reader_am10.GetDecimal(4);
						i�L������     = reader_am10.GetDecimal(5);
						s�������t     = reader_am10.GetString(6);

						//�����̔ԍX�V���̎擾
						i�J�n���[�ԍ� = i�ŏI�ԍ� + 1;
						i�I�����[�ԍ� = i�ŏI�ԍ� + i���t����;
						i�ŏI���[�ԍ� = i�J�n���[�ԍ�;
						s���t��       = s�������t;
						i�L�������N   = int.Parse(s���t��.Substring(0, 4));
						i�L��������   = int.Parse(s���t��.Substring(4, 2)) + i�L������ - 1;
						if (i�L�������� > 12)
						{
							i�L�������N++;
							i�L�������� = i�L�������� - 12;
						}
						i�L��������   = System.DateTime.DaysInMonth(decimal.ToInt32(i�L�������N), decimal.ToInt32(i�L��������));
						s�L������     = i�L�������N.ToString() + i�L��������.ToString().PadLeft(2, '0') + i�L��������.ToString().PadLeft(2, '0');

						//�̔ԊǗ��X�V���̎擾
						i�ŏI�ԍ�     = i�I�����[�ԍ�;

						sRet[1] = i�ŏI���[�ԍ�.ToString();
						intCnt_am10++;
					}
					disposeReader(reader_am10);
					reader_am10 = null;
					if (intCnt_am10 == 0)
					{
						//�Y���f�[�^���Ȃ��ꍇ�̓G���[
						throw new Exception("�Y���f�[�^������܂���");
					}
					if (i�ŏI�ԍ� > i�I���ԍ�)
					{
						i�o�^�A��++;
						if (i�o�^�A�� > i�ő�A��)
						{
							i�o�^�A�� = 1;
						}
						//�����ԍ����V�K���[�ԍ��g���擾
						string cmdQuery_am11 = "SELECT";
						cmdQuery_am11 += " AM11.�J�n���[�ԍ� \n";
						cmdQuery_am11 += " FROM �`�l�P�P�����ԍ� AM11 \n";
						cmdQuery_am11 += " WHERE AM11.�����敪 = '" + sKey[2] + "' \n";
						cmdQuery_am11 += " AND AM11.�o�^�A�� =  " + i�o�^�A�� + " \n";
						cmdQuery_am11 += " AND AM11.�폜�e�f = '0' \n";
						cmdQuery_am11 += " FOR UPDATE \n";

						OracleDataReader reader_am11 = CmdSelect(sUser, conn2, cmdQuery_am11);
						int intCnt_am11 = 0;
						if (reader_am11.Read())
						{
							i�J�n�ԍ�     = reader_am11.GetDecimal(0);
							//�̔ԊǗ��X�V���̎擾
							i�ŏI�ԍ�     = i�J�n�ԍ� + i���t���� - 1;
							//�����̔ԍX�V���̎擾
							i�J�n���[�ԍ� = i�J�n�ԍ�;
							i�I�����[�ԍ� = i�ŏI�ԍ�;
							i�ŏI���[�ԍ� = i�J�n���[�ԍ�;

							sRet[1] = i�ŏI���[�ԍ�.ToString();
							intCnt_am11++;
						}
						disposeReader(reader_am11);
						reader_am11 = null;
						if (intCnt_am11 == 0)
						{
							//�Y���f�[�^���Ȃ��ꍇ�̓G���[
							throw new Exception("�Y���f�[�^������܂���");
						}
					}
					// �̔ԊǗ��̍X�V
					string updQuery_am10 = "UPDATE �`�l�P�O�̔ԊǗ� \n";
					updQuery_am10 += " SET �o�^�A�� = " + i�o�^�A��;
					updQuery_am10 += ", �ŏI���[�ԍ� = " + i�ŏI�ԍ�;
					updQuery_am10 += ", �X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "; // �X�V����
					updQuery_am10 += ", �X�V�� = '" + sKey[3] + "' \n";                   // �X�V��
					updQuery_am10 += " WHERE �̔ԋ敪 = '" + sKey[2] + "' \n";

					CmdUpdate(sUser, conn2, updQuery_am10);
				}

				string updQuery_am12 = "";
				if (intCnt_am12 == 0)
				{
					// �����̔Ԃ̒ǉ�
					updQuery_am12  = "INSERT INTO �`�l�P�Q�����̔� \n";
					updQuery_am12 += " VALUES ('" + sKey[0] + "' ";
					updQuery_am12 +=         ",'" + sKey[1] + "' ";
					updQuery_am12 +=         ",'" + sKey[2] + "' ";
					updQuery_am12 +=         ", " + i�o�^�A��;
					updQuery_am12 +=         ", " + i�J�n���[�ԍ�;
					updQuery_am12 +=         ", " + i�I�����[�ԍ�;
					updQuery_am12 +=         ", " + i�ŏI���[�ԍ�;
					updQuery_am12 +=         ",'" + s���t�� + "' ";
					updQuery_am12 +=         ",'" + s�L������ + "' ";
					updQuery_am12 +=         ",'0' ";
					updQuery_am12 +=         ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=         ",'�o�דo�^' ";
					updQuery_am12 +=         ",'" + sKey[3] + "' ";
					updQuery_am12 +=         ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=         ",'�o�דo�^' ";
					updQuery_am12 +=         ",'" + sKey[3] + "' ";
					updQuery_am12 += " ) ";
				}
				else
				{
					// �����̔Ԃ̍X�V
					updQuery_am12  = "UPDATE �`�l�P�Q�����̔� \n";
					updQuery_am12 += " SET �o�^�A�� =  " + i�o�^�A��;
					updQuery_am12 +=      ", �J�n���[�ԍ� =  " + i�J�n���[�ԍ�;
					updQuery_am12 +=      ", �I�����[�ԍ� =  " + i�I�����[�ԍ�;
					updQuery_am12 +=      ", �ŏI���[�ԍ� =  " + sRet[1];
					updQuery_am12 +=      ", ���t�� = '" + s���t�� + "'";
					updQuery_am12 +=      ", �L������ = '" + s�L������ + "'";
					updQuery_am12 +=      ", �X�V���� =   TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') ";
					updQuery_am12 +=      ", �X�V�o�f = '�o�דo�^' ";
					updQuery_am12 +=      ", �X�V�� = '" + sKey[3] + "' \n";
					updQuery_am12 += " WHERE ����b�c = '" + sKey[0] + "' \n";
					updQuery_am12 +=   " AND ����b�c = '" + sKey[1] + "' \n";
					updQuery_am12 +=   " AND �����敪 = '" + sKey[2] + "' \n";
				}
				CmdUpdate(sUser, conn2, updQuery_am12);
				tran.Commit();
				sRet[0] = "����I��";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �����ԍ��X�V
		 * �����F����b�c�A����b�c�A�o�^���A�W���[�i���m�n�A�����ԍ��A�X�V��
		 * �@�@�@����f�[�^�A���p�ҕ���X���b�c
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2print\Service1.asmx.cs(963):
		*/
		//		[WebMethod]
		private String[] Set_InvoiceNo2(string[] sUser, string[] sKey, string[] sPrintData, string sTensyo)
		{
			logWriter(sUser, INF, "�����ԍ��X�V�Q�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string s�����ԍ� = "";
				sbQuery.Append("SELECT �����ԍ� \n");
				sbQuery.Append(" FROM  \"�r�s�O�P�o�׃W���[�i��\" \n");
				sbQuery.Append(" WHERE ����b�c = '" + sKey[0] + "' \n");
				sbQuery.Append(" AND ����b�c = '" + sKey[1] + "' \n");
				sbQuery.Append(" AND �o�^��   = '" + sKey[2] + "' \n");
				sbQuery.Append(" AND \"�W���[�i���m�n\" = '" + sKey[3] + "' \n");
				sbQuery.Append(" AND �폜�e�f = '0' \n");
				sbQuery.Append(" FOR UPDATE \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				if(reader.Read())
				{
					s�����ԍ� = reader.GetString(0).TrimEnd();
				}
				disposeReader(reader);
				reader = null;
				sbQuery = null;
				if(s�����ԍ�.Length > 0)
				{
					tran.Commit();
					sRet[0] = "�̔ԍς�";
					sRet[1] = s�����ԍ�;
					logWriter(sUser, INF, "�����ԍ��X�V�Q�@�����ԍ��X�V��["+s�����ԍ�+"]");
					return sRet;
				}
				// �����ԍ��`�F�b�N
				String[] sGetKey = new string[4];
				sGetKey[0] = sKey[0];
				sGetKey[1] = sTensyo; // ���p�ҕ���X���b�c
				sGetKey[2] = sPrintData[32]; //�����敪 + "0" or "1"
				if(sPrintData[14].Substring(1, 3) == "047")
				{
					sGetKey[2] = sPrintData[32].Substring(0,1) + "G"; //�����敪 + "G"
				}
				sGetKey[3] = sUser[1];
				String[] sGetData = this.Get_InvoiceNo(sUser, sGetKey);
				if(sGetData[0].Length != 4)
				{
					tran.Commit();
					sRet[0] = sGetData[0];
					return sRet;
				}
				//�����ԍ��̃Z�b�g
				sPrintData[11] = sGetData[1].PadLeft(14, '0');
				//�`�F�b�N�f�W�b�g�i�V�Ŋ������]��j�̕t��
				sPrintData[11] = sPrintData[11] + (long.Parse(sPrintData[11]) % 7).ToString();

				// �o�׃W���[�i���̍X�V
				string cmdQuery  = "UPDATE \"�r�s�O�P�o�׃W���[�i��\" \n";
				cmdQuery += " SET �����ԍ� = '"  + sPrintData[11] + "' ";                     // �����ԍ�
				cmdQuery += " WHERE ����b�c       = '" + sKey[0] + "' \n";
				cmdQuery +=   " AND ����b�c       = '" + sKey[1] + "' \n";
				cmdQuery +=   " AND �o�^��         = '" + sKey[2] + "' \n";
				cmdQuery +=   " AND �W���[�i���m�n = '" + sKey[3] + "' \n";
				cmdQuery +=   " AND �폜�e�f       = '0' \n";

				CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "����I��";
				logWriter(sUser, INF, "�����ԍ��X�V�Q�@�����ԍ��X�V["+sPrintData[11]+"]");
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
		// MOD 2011.03.25 ���s�j���� �����ԍ��̏㏑���h�~ END

		/*********************************************************************
		 * ���X�擾
		 * �����F�ב��l�b�c
		 * �ߒl�F�X�e�[�^�X�A�X���b�c�A�X�����A�s���{���b�c�A�s�撬���b�c�A�厚�ʏ̂b�c
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(1851):
		*/
		private String[] Get_hatuten(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[4];

			string cmdQuery = "SELECT Y.�X���b�c, T.�X����, Y.�s���{���b�c, Y.�s�撬���b�c, Y.�厚�ʏ̂b�c \n"
				+ " FROM �b�l�O�Q���� B, \n"
				+      " �b�l�P�S�X�֔ԍ��i Y, \n" // ���q�^���Ή�
				+      " �b�l�P�O�X�� T \n"
				+ " WHERE B.����b�c = '" + sKcode + "' \n"
				+ " AND B.����b�c = '" + sBcode + "' \n"
				+ " AND B.�폜�e�f = '0' \n"
				+ " AND B.�X�֔ԍ� = Y.�X�֔ԍ� \n"
				+ " AND Y.�X���b�c = T.�X���b�c \n"
				+ " AND T.�폜�e�f = '0' \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[1] = reader.GetString(0).Trim(); // �X���b�c
				sRet[2] = reader.GetString(1).Trim(); // �X����
				sRet[3] = reader.GetString(2).Trim()  // �Z���b�c
					+ reader.GetString(3).Trim()
					+ reader.GetString(4).Trim();

				sRet[0] = " ";
			}
			else
			{
				sRet[0] = "���X�����߂��܂���ł���";
				sRet[1] = "0000";
				sRet[2] = " ";
				sRet[3] = " ";
			}
			disposeReader(reader);
			reader = null;
			
			return sRet;
		}

		/*********************************************************************
		 * ���X�擾
		 * �����F�ב��l�b�c
		 * �ߒl�F�X�e�[�^�X�A�X���b�c�A�X�����A�s���{���b�c�A�s�撬���b�c�A�厚�ʏ̂b�c
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(1932):
		*/
		[WebMethod]
		public String[] Get_hatuten2(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "���X�擾�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery = "SELECT Y.�X���b�c, T.�X����, Y.�s���{���b�c, Y.�s�撬���b�c, Y.�厚�ʏ̂b�c \n"
					+ " FROM �b�l�O�Q���� B, \n"
					+      " �b�l�P�S�X�֔ԍ��i Y, \n" // ���q�^���Ή�
					+      " �b�l�P�O�X�� T \n"
					+ " WHERE B.����b�c = '" + sKcode + "' \n"
					+ " AND B.����b�c = '" + sBcode + "' \n"
					+ " AND B.�폜�e�f = '0' \n"
					+ " AND B.�X�֔ԍ� = Y.�X�֔ԍ� \n"
					+ " AND Y.�X���b�c = T.�X���b�c \n"
					+ " AND T.�폜�e�f = '0' \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				if(reader.Read())
				{
					sRet[1] = reader.GetString(0).Trim();
					sRet[2] = reader.GetString(1).Trim();
					sRet[3] = reader.GetString(2).Trim()
						+ reader.GetString(3).Trim()
						+ reader.GetString(4).Trim();

					sRet[0] = "����I��";
				}
				else
				{
					sRet[0] = "�Y���f�[�^������܂���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �W��X�擾
		 * �����F����b�c�A����b�c
		 * �ߒl�F�X�e�[�^�X�A�W��X�b�c
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(2070):
		*/
		private String[] Get_syuuyakuten(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[2];

			string cmdQuery = "SELECT T.�W��X�b�c \n"
				+ " FROM �b�l�O�Q���� B,�b�l�P�O�X�� T, \n"
				+        "�b�l�P�S�X�֔ԍ��i Y  \n" // ���q�^���Ή�
				+ " WHERE B.����b�c   = '" + sKcode + "' \n"
				+ "   AND B.����b�c   = '" + sBcode + "' \n"
				+ "   AND B.�폜�e�f     = '0' \n"
				+    "AND B.�X�֔ԍ� = Y.�X�֔ԍ� \n"
				+    "AND Y.�X���b�c     = T.�X���b�c \n"
				+ "   AND T.�폜�e�f     = '0'";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0).Trim();
			}
			else
			{
				sRet[0] = "�W��X�����߂��܂���ł���";
				sRet[1] = "0000";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * �W��X�擾
		 * �����F����b�c�A����b�c
		 * �ߒl�F�X�e�[�^�X�A�W��X�b�c
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(2112):
		*/
		[WebMethod]
		public String[] Get_syuuyakuten2(string[] sUser, string sKcode, string sBcode)
		{
			logWriter(sUser, INF, "�W��X�擾�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery = "SELECT T.�W��X�b�c \n"
					+ " FROM �b�l�O�Q���� B,�b�l�P�O�X�� T, \n"
					+        "�b�l�P�S�X�֔ԍ��i Y  \n" // ���q�^���Ή�
					+ " WHERE B.����b�c   = '" + sKcode + "' \n"
					+ "   AND B.����b�c   = '" + sBcode + "' \n"
					+ "   AND B.�폜�e�f     = '0' \n"
					+    "AND B.�X�֔ԍ� = Y.�X�֔ԍ� \n"
					+    "AND Y.�X���b�c     = T.�X���b�c \n"
					+ "   AND T.�폜�e�f     = '0'";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				if(reader.Read())
				{
					sRet[0] = "����I��";
					sRet[1] = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "�Y���f�[�^������܂���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ���X�擾
		 * �@�@�r�l�O�Q�׎�l�A�b�l�P�S�X�֔ԍ��A�b�l�P�T���X��\���A�b�l�P�X�X�֏Z��
		 *     �̂S�}�X�^���g�p���Ē��X�R�[�h�����肷��B
		 * �����F����R�[�h�A����R�[�h�A�׎�l�R�[�h�A�X�֔ԍ��A�Z���A����
		 * �ߒl�F�X�e�[�^�X�A�X���b�c�A�X�����A�Z���b�c
		 *
		 * Create : 2008.06.12 kcl)�X�{
		 * �@�@�@�@�@�@Get_tyakuten �����ɍ쐬
		 * Modify : 2008.12.24 kcl)�X�{
		 * �@�@�@�@�@�@�b�l�P�X�̌������@��ύX�A����ю�������̌�����ǉ�
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(4769):
		*/
		private String[] Get_tyakuten3(string[] sUser, OracleConnection conn2, 
			string sKaiinCode, string sBumonCode, string sNiukeCode, 
			string sYuubin, string sJuusyo, string sShimei)
		{
			string [] sRet = new string [4];		// �߂�l
			string cmdQuery;						// SQL��
			OracleDataReader reader;
			string tenCD       = string.Empty;		// �X���R�[�h
			string tenName     = string.Empty;		// �X����
			string juusyoCD    = string.Empty;		// �Z���R�[�h
			string address     = string.Empty;		// �Z��
			string niuJuusyoCD = string.Empty;		// �׎�l�}�X�^�̏Z���R�[�h

			///
			/// ����P�i�K��
			/// �׎�l�}�X�^�̒��X�R�[�h������
			/// 
			string niuCode = sNiukeCode.Trim();
			if (niuCode.Length > 0) 
			{
				// SQL��
				cmdQuery
					= "SELECT SM02.����b�c, NVL(CM10.�X����, ' '), SM02.�Z���b�c \n"
					+ "  FROM �r�l�O�Q�׎�l SM02 \n"
					+ "  LEFT OUTER JOIN �b�l�P�O�X�� CM10 \n"
					+ "    ON SM02.����b�c   = CM10.�X���b�c \n"
					+ "   AND CM10.�폜�e�f   = '0' \n"
					+ " WHERE SM02.����b�c   = '" + sKaiinCode + "' \n"
					+ "   AND SM02.����b�c   = '" + sBumonCode + "' \n"
					+ "   AND SM02.�׎�l�b�c = '" + sNiukeCode + "' \n"
					+ "   AND ( LENGTH(TRIM(SM02.����b�c)) > 0 \n"
					+ "      OR LENGTH(TRIM(SM02.�Z���b�c)) > 0 ) \n"
					+ "   AND SM02.�폜�e�f   = '0' \n";

				// SQL���s
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// �f�[�^�擾
				if (reader.Read())
				{
					// �Y���f�[�^����

					// �f�[�^�擾
					tenCD    = reader.GetString(0).Trim();		// �X���R�[�h
					tenName  = reader.GetString(1).Trim();		// �X����
					juusyoCD = reader.GetString(2).Trim();		// �Z���R�[�h

					if (tenCD.Length > 0) 
					{
						// �׎�l�}�X�^�̒��X�R�[�h�����͂���Ă���ꍇ

						// �Z���R�[�h�̐ݒ�
						if (juusyoCD.Length == 0) 
						{
							// �׎�l�}�X�^�̏Z���R�[�h���󗓂̏ꍇ

							// �X�֔ԍ��}�X�^����擾
							string [] sResult = this.Get_juusyoCode(sUser, conn2, sYuubin);
							if (sResult[0] == " ") 
								juusyoCD = sResult[1];
						}

						// �߂�l���Z�b�g
						sRet[0] = " ";
						sRet[1] = tenCD;
						sRet[2] = tenName;
						sRet[3] = juusyoCD;

						// �I������
						disposeReader(reader);
						reader = null;
					
						return sRet;
					} 
					else
					{
						// �׎�l�}�X�^�ɏZ���R�[�h�݂̂����͂���Ă���ꍇ

						// �׎�l�}�X�^�̏Z���R�[�h���Ƃ��Ă���
						niuJuusyoCD = juusyoCD;
					}
				}

				// �I������
				disposeReader(reader);
				reader = null;
			}

			///
			/// ����Q�i�K��
			/// �X�֔ԍ��}�X�^���璅�X�R�[�h������
			///
			cmdQuery
				= "SELECT CM15.�X�֔ԍ� \n"
				+ " FROM �b�l�P�T���X��\���i CM15 \n" // ���q�^���Ή�
				+ " WHERE CM15.�X�֔ԍ� = '" + sYuubin + "' \n"
				+ "   AND CM15.�폜�e�f = '0' \n";

			// SQL���s
			reader = CmdSelect(sUser, conn2, cmdQuery);
			// �f�[�^�擾
			if (reader.Read())
			{
				; // �X�֔ԍ��}�X�^�͌������Ȃ�
			}
			else
			{
				// �I������
				disposeReader(reader);
				reader = null;
				// SQL��
				cmdQuery
					= "SELECT CM14.�X���b�c, CM10.�X����, CM14.�s���{���b�c || CM14.�s�撬���b�c || CM14.�厚�ʏ̂b�c \n"
					+ "  FROM �b�l�P�S�X�֔ԍ��i CM14 \n" // ���q�^���Ή�
					+ " INNER JOIN �b�l�P�O�X�� CM10 \n"
					+ "    ON CM14.�X���b�c = CM10.�X���b�c \n"
					+ "   AND CM10.�폜�e�f = '0' \n"
					+ " WHERE CM14.�X�֔ԍ� = '" + sYuubin + "' \n"
					+ "   AND LENGTH(TRIM(CM14.�X���b�c)) > 0 \n"
					+ "   AND CM14.�폜�e�f = '0' \n";

				// SQL���s
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// �f�[�^�擾
				if (reader.Read())
				{
					// �Y���f�[�^����

					// �f�[�^�擾
					tenCD    = reader.GetString(0).Trim();		// �X���R�[�h
					tenName  = reader.GetString(1).Trim();		// �X����
					juusyoCD = reader.GetString(2).Trim();		// �Z���R�[�h

					// �߂�l���Z�b�g
					sRet[0] = " ";
					sRet[1] = tenCD;
					sRet[2] = tenName;
					sRet[3] = (niuJuusyoCD.Length > 0) ? niuJuusyoCD : juusyoCD;
					// ���� �׎�l�}�X�^�̏Z���R�[�h��D�悷��

					// �I������
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
				else 
				{
					// �b�l�P�S�ɊY���f�[�^�Ȃ�

					// �߂�l���Z�b�g
					sRet[0] = "���͂��ꂽ���͂���(�X�֔ԍ�)�ł͔z�B�X�����߂��܂���ł���";
					sRet[1] = "0000";
					sRet[2] = " ";
					sRet[3] = " ";

					// �I������
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
			}
			// �I������
			disposeReader(reader);
			reader = null;

			///
			/// ����R�i�K��
			/// �X�֏Z���}�X�^���璅�X�R�[�h������
			/// 
			// SQL��
			cmdQuery
				= "SELECT CM19.�X���b�c, CM10.�X����, CM19.�Z���b�c, CM19.�Z�� \n"
				+ "  FROM �b�l�P�X�X�֏Z���i CM19 \n" // ���q�^���Ή�
				+ " INNER JOIN �b�l�P�O�X�� CM10 \n"
				+ "    ON CM19.�X���b�c = CM10.�X���b�c \n"
				+ "   AND CM10.�폜�e�f = '0' \n"
				+ " WHERE CM19.�X�֔ԍ� = '" + sYuubin + "' \n"
				+ "   AND CM19.�폜�e�f = '0' \n"
				+ " ORDER BY "
				+ "       LENGTH(TRIM(CM19.�Z��)) DESC \n"
				;

			// SQL���s
			reader = CmdSelect(sUser, conn2, cmdQuery);

			// �f�[�^�擾
			while (reader.Read()) 
			{
				// �Z���̎擾
				address = reader.GetString(3).Trim();

				if (sShimei == null) sShimei = " ";

				// �Z���E�����̃`�F�b�N
				if ((sJuusyo.IndexOf(address) >= 0) ||
					(sShimei.IndexOf(address) >= 0))
				{
					// �f�[�^�擾
					tenCD    = reader.GetString(0).Trim();	// �X���R�[�h
					tenName  = reader.GetString(1).Trim();	// �X����
					juusyoCD = reader.GetString(2).Trim();	// �Z���R�[�h

					// �߂�l���Z�b�g
					sRet[0] = " ";
					sRet[1] = tenCD;
					sRet[2] = tenName;
					sRet[3] = (niuJuusyoCD.Length > 0) ? niuJuusyoCD : juusyoCD;
					// ���� �׎�l�}�X�^�̏Z���R�[�h��D�悷��

					// �I������
					disposeReader(reader);
					reader = null;
			
					return sRet;
				}
			}

			// �I������
			disposeReader(reader);
			reader = null;

			// �Y���f�[�^��
			sRet[0] = " ";
			sRet[1] = " ";
			sRet[2] = " ";
			sRet[3] = " ";
			
			return sRet;
		}

		/*********************************************************************
		 * �Z���R�[�h�擾
		 * �@�@�b�l�P�S�X�֔ԍ����g�p���āA�X�֔ԍ�����Z���R�[�h���擾����B
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�Z���b�c
		 *
		 * Create : 2008.06.16 kcl)�X�{
		 * �@�@�@�@�@�@�V�K�쐬
		 * Modify : 
		 *********************************************************************/
		private String[] Get_juusyoCode(string[] sUser, OracleConnection conn2, 
			string sYuubin)
		{
			string [] sRet = new string [2];	// �߂�l
			string cmdQuery;					// SQL��
			OracleDataReader reader;

			// SQL��
			cmdQuery
				= "SELECT CM14.�s���{���b�c || CM14.�s�撬���b�c || CM14.�厚�ʏ̂b�c \n"
				+ "  FROM �b�l�P�S�X�֔ԍ� CM14 \n"
				+ " WHERE CM14.�X�֔ԍ� = '" + sYuubin + "' \n"
				+ "   AND CM14.�폜�e�f = '0' \n";

			// SQL���s
			reader = CmdSelect(sUser, conn2, cmdQuery);

			// �f�[�^�擾
			if (reader.Read())
			{
				// �Y���f�[�^����
				sRet[0] = " ";							// �X�e�[�^�X
				sRet[1] = reader.GetString(0).Trim();	// �Z���R�[�h
			} 
			else
			{
				// �Y���f�[�^��
				sRet[0] = "���͂��ꂽ�X�֔ԍ��ł͏Z���R�[�h�����߂��܂���ł���";
				sRet[1] = " ";
			}

			// �I������
			disposeReader(reader);
			reader = null;
			
			return sRet;
		}

		/*********************************************************************
		 * �o�׃f�[�^�X�V
		 * �����F����b�c�A����b�c�A�o�ד�...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(1161):
		*/
		[WebMethod]
		public String[] Upd_syukka2(string[] sUser, string[] sData, string sNo)
		{
			logWriter(sUser, INF, "�o�׍X�V�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			decimal d����;
			string s����v = " ";
			try
			{
				//�o�ד��`�F�b�N
				string[] sSyukkabi = Get_bumonsyukka(sUser, conn2, sData[0], sData[1]);
				sRet[0] = sSyukkabi[0];
				if(sRet[0] != " ") return sRet;
				if(int.Parse(sData[2]) < int.Parse(sSyukkabi[1]))
				{
					sRet[0] = "1";
					sRet[1] = sSyukkabi[1];
					return sRet;
				}

				//�ב��l�b�c���݃`�F�b�N
				string cmdQuery
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					//					= "SELECT ���Ӑ�b�c, ���Ӑ敔�ۂb�c \n"
					//					+ "  FROM �r�l�O�P�ב��l \n"
					//					+ " WHERE ����b�c   = '" + sData[0]  +"' \n"
					//					+ "   AND ����b�c   = '" + sData[1]  +"' \n"
					//					+ "   AND �ב��l�b�c = '" + sData[15] +"' \n"
					//					+ "   AND �폜�e�f   = '0'";
					= "SELECT SM01.���Ӑ�b�c, SM01.���Ӑ敔�ۂb�c \n"
					+ "     , NVL(CM01.�ۗ�����e�f,'0') \n"
					+ "  FROM �r�l�O�P�ב��l SM01 \n"
					+ "     , �b�l�O�P��� CM01 \n"
					+ " WHERE SM01.����b�c   = '" + sData[0]  +"' \n"
					+ "   AND SM01.����b�c   = '" + sData[1]  +"' \n"
					+ "   AND SM01.�ב��l�b�c = '" + sData[15] +"' \n"
					+ "   AND SM01.�폜�e�f   = '0' \n"
					+ "   AND SM01.����b�c   = CM01.����b�c(+) \n"
					;
				string s�d�ʓ��͐��� = "0";
				// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if(reader.Read())
				{
					d���� = 1;
					sData[16] = reader.GetString(0);
					sData[17] = reader.GetString(1);
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					s�d�ʓ��͐��� = reader.GetString(2);
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				}
				else
				{
					d���� = 0;
				}
				disposeReader(reader);
				reader = null;

				if(d���� == 0)
				{
					sRet[0] = "0";
				}
				else
				{
					cmdQuery
						= "SELECT SM04.���Ӑ敔�ۖ� \n"
						+ " FROM �b�l�O�Q���� CM02 \n"
						+    " , �r�l�O�S������ SM04 \n"
						+ " WHERE CM02.����b�c = '" + sData[0] + "' \n"
						+  " AND CM02.����b�c = '" + sData[1] + "' \n"
						+  " AND CM02.�폜�e�f = '0' \n"
						+  " AND SM04.����b�c = CM02.����b�c \n"
						+  " AND SM04.�X�֔ԍ� = CM02.�X�֔ԍ� \n"
						+  " AND SM04.���Ӑ�b�c = '" + sData[16] + "' \n"
						+  " AND SM04.���Ӑ敔�ۂb�c = '" + sData[17] + "' \n"
						+  " AND SM04.�폜�e�f = '0' \n"
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

					//����v�擾
					if(sData[4] != " ")
					{
						cmdQuery
							= "SELECT NVL(����v,' ') \n"
							+ "  FROM �r�l�O�Q�׎�l \n"
							+ " WHERE ����b�c   = '" + sData[0] +"' \n"
							+ "   AND ����b�c   = '" + sData[1] +"' \n"
							+ "   AND �׎�l�b�c = '" + sData[4] +"' \n"
							+ "   AND �폜�e�f   = '0'";

						reader = CmdSelect(sUser, conn2, cmdQuery);

						bool bRead = reader.Read();
						if(bRead == true)
							s����v   = reader.GetString(0);

						disposeReader(reader);
						reader = null;
						cmdQuery
							= "UPDATE �r�l�O�Q�׎�l \n"
							+ " SET �o�^�o�f = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ " WHERE ����b�c = '" + sData[0] +"' \n"
							+ " AND ����b�c   = '" + sData[1] +"' \n"
							+ " AND �׎�l�b�c = '" + sData[4] +"' \n"
							+ " AND �폜�e�f   = '0'";
						try
						{
							int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
						}
						catch(Exception)
						{
							;
						}
					}

					//���X�擾
					string[] sTyakuten = Get_tyakuten3(sUser, conn2, 
						sData[0], sData[1], sData[4], 
						sData[13] + sData[14], sData[8] + sData[9] + sData[10], sData[11] + sData[12]);
					sRet[0] = sTyakuten[0];
					if(sRet[0] != " ") return sRet;
					string s���X�b�c = (sTyakuten[1].Length > 0) ? sTyakuten[1] : " ";
					string s���X��   = (sTyakuten[2].Length > 0) ? sTyakuten[2] : " ";
					string s�Z���b�c = (sTyakuten[3].Length > 0) ? sTyakuten[3] : " ";

					//���X�擾
					string[] sHatuten = Get_hatuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sHatuten[0];
					if(sRet[0] != " ") return sRet;
					string s���X�b�c = sHatuten[1];
					string s���X��   = sHatuten[2];

					//�W�דX�擾
					string[] sSyuyaku = Get_syuuyakuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sSyuyaku[0];
					if(sRet[0] != " ") return sRet;
					string s�W��X�b�c = sSyuyaku[1];

					//�d���b�c�擾
					string s�d���b�c = " ";
					if(s���X�b�c.Trim().Length > 0 && s���X�b�c.Trim().Length > 0)
					{
						string[] sRetSiwake = Get_siwake(sUser, conn2, s���X�b�c, s���X�b�c);
						s�d���b�c = sRetSiwake[1];
					}
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					if(s�d�ʓ��͐��� == "0")
					{
						sData[38] = "0"; // �ː�
						sData[20] = "0"; // �d��
					}
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
					string s�i���L���S = (sData.Length > 43) ? sData[43] : " ";
					string s�i���L���T = (sData.Length > 44) ? sData[44] : " ";
					string s�i���L���U = (sData.Length > 45) ? sData[45] : " ";
					if(s�i���L���S.Length == 0) s�i���L���S = " ";
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END

					cmdQuery 
						= "UPDATE \"�r�s�O�P�o�׃W���[�i��\" \n"
						+    "SET �o�ד�             = '" + sData[2]  +"', \n"
						+        "���q�l�o�הԍ�     = '" + sData[3]  +"',"
						+        "�׎�l�b�c         = '" + sData[4]  +"',"
						+        "�d�b�ԍ��P         = '" + sData[5]  +"', \n"
						+        "�d�b�ԍ��Q         = '" + sData[6]  +"',"
						+        "�d�b�ԍ��R         = '" + sData[7]  +"',"
						+        "�Z���b�c           = '" + s�Z���b�c +"', \n"
						+        "�Z���P             = '" + sData[8]  +"',"
						+        "�Z���Q             = '" + sData[9]  +"',"
						+        "�Z���R             = '" + sData[10] +"', \n"
						+        "���O�P             = '" + sData[11] +"',"
						+        "���O�Q             = '" + sData[12] +"',"
						+        "�X�֔ԍ�           = '" + sData[13] + sData[14] +"', \n"
						+        "���X�b�c           = '" + s���X�b�c +"',"
						+        "���X��             = '" + s���X��   +"',"
						+        "����v             = '" + s����v   +"', \n"
						+        "�ב��l�b�c         = '" + sData[15] +"',"
						+        "�ב��l������       = '" + sData[37] +"',"
						+        "�W��X�b�c         = '" + s�W��X�b�c +"', \n"
						+        "���X�b�c           = '" + s���X�b�c +"',"
						+        "���X��             = '" + s���X��   +"',"
						+        "���Ӑ�b�c         = '" + sData[16] +"', \n"
						+        "���ۂb�c           = '" + sData[17] +"',"
						+        "���ۖ�             = '" + sData[18] +"',"
						+        "��               =  " + sData[19] +", \n"
						+        "�ː�               =  " + sData[38] +","
						+        "�d��               =  " + sData[20] +","
						+        "�w���             = '" + sData[21] +"',"
						+        "�w����敪         = '" + sData[41] +"',"
						+        "�A���w���b�c�P     = '" + sData[39] +"',"
						+        "�A���w���P         = '" + sData[22] +"', \n"
						+        "�A���w���b�c�Q     = '" + sData[40] +"',"
						+        "�A���w���Q         = '" + sData[23] +"',"
						+        "�i���L���P         = '" + sData[24] +"',"
						+        "�i���L���Q         = '" + sData[25] +"', \n"
						+        "�i���L���R         = '" + sData[26] +"',"
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
						+        "�i���L���S         = '" + s�i���L���S +"', \n"
						+        "�i���L���T         = '" + s�i���L���T +"',"
						+        "�i���L���U         = '" + s�i���L���U +"', \n"
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
						+        "�ی����z           =  " + sData[28] +","
						+        "�d���b�c           = '" + s�d���b�c + "', \n"
						+        "����󔭍s�ςe�f   = '0', \n"
						+        "���M�ςe�f         = '0',"
						+        "���               = '01',"
						+        "�ڍ׏��           = '  ', \n"
						+        "�X�V�o�f           = '" + sData[32] +"',"
						+        "�X�V��             = '" + sData[33] +"', \n"
						+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE ����b�c           = '" + sData[0]  +"' \n"
						+ "   AND ����b�c           = '" + sData[1]  +"' \n"
						+ "   AND �o�^��             = '" + sData[35] +"' \n"
						+ "   AND \"�W���[�i���m�n\" = '" + sData[34] +"' \n"
						+ "   AND �X�V����           =  " + sData[36] +"";
					logWriter(sUser, INF, "�o�׍X�V["+sData[1]+"]["+sData[35]+"]["+sData[34]+"]:["+sNo+"]");

					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					tran.Commit();
					if(iUpdRow == 0)
						sRet[0] = "�f�[�^�ҏW���ɑ��̒[�����X�V����Ă��܂��B\r\n�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
					else
						sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �o�׃f�[�^�o�^
		 * �����F����b�c�A����b�c�A�o�ד�...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(736):
		*/
		[WebMethod]
		public String[] Ins_syukka(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "�o�דo�^�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			decimal d����;
			string s����v = " ";
			string s�o�^��;
			int i�Ǘ��m�n;
			string s���t;
			try
			{
				//�o�ד��`�F�b�N
				string[] sSyukkabi = Get_bumonsyukka(sUser, conn2, sData[0], sData[1]);
				sRet[0] = sSyukkabi[0];
				if(sRet[0] != " ") return sRet;
				if(int.Parse(sData[2]) < int.Parse(sSyukkabi[1]))
				{
					sRet[0] = "1";
					sRet[1] = sSyukkabi[1];
					return sRet;
				}

				//�ב��l�b�c���݃`�F�b�N
				string cmdQuery
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					//					= "SELECT ���Ӑ�b�c, ���Ӑ敔�ۂb�c \n"
					//					+ "  FROM �r�l�O�P�ב��l \n"
					//					+ " WHERE ����b�c   = '" + sData[0]  +"' \n"
					//					+ "   AND ����b�c   = '" + sData[1]  +"' \n"
					//					+ "   AND �ב��l�b�c = '" + sData[15] +"' \n"
					//					+ "   AND �폜�e�f   = '0'";
					= "SELECT SM01.���Ӑ�b�c, SM01.���Ӑ敔�ۂb�c \n"
					+ "     , NVL(CM01.�ۗ�����e�f,'0') \n"
					+ "  FROM �r�l�O�P�ב��l SM01 \n"
					+ "     , �b�l�O�P��� CM01 \n"
					+ " WHERE SM01.����b�c   = '" + sData[0]  +"' \n"
					+ "   AND SM01.����b�c   = '" + sData[1]  +"' \n"
					+ "   AND SM01.�ב��l�b�c = '" + sData[15] +"' \n"
					+ "   AND SM01.�폜�e�f   = '0' \n"
					+ "   AND SM01.����b�c   = CM01.����b�c(+) \n"
					;
				string s�d�ʓ��͐��� = "0";
				// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				if(reader.Read())
				{
					d���� = 1;
					sData[16] = reader.GetString(0);
					sData[17] = reader.GetString(1);
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					s�d�ʓ��͐��� = reader.GetString(2);
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				}
				else
				{
					d���� = 0;
				}
				disposeReader(reader);
				reader = null;
				if(d���� == 0)
				{
					sRet[0] = "0";
				}
				else
				{
					cmdQuery
						= "SELECT SM04.���Ӑ敔�ۖ� \n"
						+ " FROM �b�l�O�Q���� CM02 \n"
						+    " , �r�l�O�S������ SM04 \n"
						+ " WHERE CM02.����b�c = '" + sData[0] + "' \n"
						+  " AND CM02.����b�c = '" + sData[1] + "' \n"
						+  " AND CM02.�폜�e�f = '0' \n"
						+  " AND SM04.�X�֔ԍ� = CM02.�X�֔ԍ� \n"
						+  " AND SM04.���Ӑ�b�c = '" + sData[16] + "' \n"
						+  " AND SM04.���Ӑ敔�ۂb�c = '" + sData[17] + "' \n"
						// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
						+  " AND SM04.����b�c = CM02.����b�c \n"
						// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
						+  " AND SM04.�폜�e�f = '0' \n"
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

					//����v�擾
					if(sData[4] != " ")
					{
						cmdQuery
							= "SELECT NVL(����v,' ') \n"
							+ "  FROM �r�l�O�Q�׎�l \n"
							+ " WHERE ����b�c   = '" + sData[0] +"' \n"
							+ "   AND ����b�c   = '" + sData[1] +"' \n"
							+ "   AND �׎�l�b�c = '" + sData[4] +"' \n"
							+ "   AND �폜�e�f   = '0'";

						reader = CmdSelect(sUser, conn2, cmdQuery);
						bool bRead = reader.Read();
						if(bRead == true)
							s����v   = reader.GetString(0);

						disposeReader(reader);
						reader = null;
						cmdQuery
							= "UPDATE �r�l�O�Q�׎�l \n"
							+ " SET �o�^�o�f = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ " WHERE ����b�c = '" + sData[0] +"' \n"
							+ " AND ����b�c   = '" + sData[1] +"' \n"
							+ " AND �׎�l�b�c = '" + sData[4] +"' \n"
							+ " AND �폜�e�f   = '0'";
						try
						{
							int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
						}
						catch(Exception)
						{
							;
						}
					}

					//�W���[�i���m�n�擾
					cmdQuery
						= "SELECT \"�W���[�i���m�n�o�^��\",\"�W���[�i���m�n�Ǘ�\", \n"
						+ "       TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+ "  FROM �b�l�O�Q���� \n"
						+ " WHERE ����b�c = '" + sData[0] +"' \n"
						+ "   AND ����b�c = '" + sData[1] +"' \n"
						+ "   AND �폜�e�f = '0'"
						+ "   FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					reader.Read();
					s�o�^��   = reader.GetString(0).Trim();
					i�Ǘ��m�n = reader.GetInt32(1);
					s���t     = reader.GetString(2);

					if(s�o�^�� == s���t)
						i�Ǘ��m�n++;
					else
					{
						s�o�^�� = s���t;
						i�Ǘ��m�n = 1;
					}

					cmdQuery 
						= "UPDATE �b�l�O�Q���� \n"
						+    "SET \"�W���[�i���m�n�o�^��\"  = '" + s�o�^�� +"', \n"
						+        "\"�W���[�i���m�n�Ǘ�\"    = " + i�Ǘ��m�n +", \n"
						+        "�X�V�o�f                  = '" + sData[32] +"', \n"
						+        "�X�V��                    = '" + sData[33] +"', \n"
						+        "�X�V����                  =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE ����b�c       = '" + sData[0] +"' \n"
						+ "   AND ����b�c       = '" + sData[1] +"' \n"
						+ "   AND �폜�e�f = '0'";

					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					disposeReader(reader);
					reader = null;

					//���X�擾
					string[] sTyakuten = Get_tyakuten3(sUser, conn2, 
						sData[0], sData[1], sData[4], 
						sData[13] + sData[14], sData[8] + sData[9] + sData[10], sData[11] + sData[12]);
					sRet[0] = sTyakuten[0];
					if(sRet[0] != " ") return sRet;
					string s���X�b�c = (sTyakuten[1].Length > 0) ? sTyakuten[1] : " ";
					string s���X��   = (sTyakuten[2].Length > 0) ? sTyakuten[2] : " ";
					string s�Z���b�c = (sTyakuten[3].Length > 0) ? sTyakuten[3] : " ";

					//���X�擾
					string[] sHatuten = Get_hatuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sHatuten[0];
					if(sRet[0] != " ") return sRet;
					string s���X�b�c = sHatuten[1];
					string s���X��   = sHatuten[2];

					//�W�דX�擾
					string[] sSyuyaku = Get_syuuyakuten(sUser, conn2, sData[0], sData[1]);
					sRet[0] = sSyuyaku[0];
					if(sRet[0] != " ") return sRet;
					string s�W��X�b�c = sSyuyaku[1];

					//�d���b�c�擾
					string s�d���b�c = " ";
					if(s���X�b�c.Trim().Length > 0 && s���X�b�c.Trim().Length > 0)
					{
						string[] sRetSiwake = Get_siwake(sUser, conn2, s���X�b�c, s���X�b�c);
						s�d���b�c = sRetSiwake[1];
					}

					// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� START
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
					//					// �����O�Q�ɍː�����яd�ʂ̎Q�l�l������
					//					string s�ː� = "";
					//					string s�d�� = "";
					//					string s�ː��d�� = "";
					//					try{
					//						s�ː� = sData[38].Trim().PadLeft(5,'0');
					//						s�d�� = sData[20].Trim().PadLeft(5,'0');
					//						s�ː��d�� = s�ː�.Substring(0,5)
					//									+ s�d��.Substring(0,5);
					//					}catch(Exception){
					//					}
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					///					string s�d�ʓ��͐��� = (sData.Length > 42) ? sData[42] : "0";
					///					if(s�d�ʓ��͐��� != "1"){
					///					string s�d�ʓ��͐��� = (sData.Length > 42) ? sData[42] : " ";
					if(s�d�ʓ��͐��� == "0")
					{
						// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						sData[38] = "0"; // �ː�
						sData[20] = "0"; // �d��
						// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					}
					// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� END
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
					string s�i���L���S = (sData.Length > 43) ? sData[43] : " ";
					string s�i���L���T = (sData.Length > 44) ? sData[44] : " ";
					string s�i���L���U = (sData.Length > 45) ? sData[45] : " ";
					if(s�i���L���S.Length == 0) s�i���L���S = " ";
					// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
					cmdQuery 
						= "INSERT INTO \"�r�s�O�P�o�׃W���[�i��\" \n"
						+ "(����b�c, ����b�c, �o�^��, \"�W���[�i���m�n\", �o�ד� \n"
						+ ", ���q�l�o�הԍ�, �׎�l�b�c \n"
						+ ", �d�b�ԍ��P, �d�b�ԍ��Q, �d�b�ԍ��R, �e�`�w�ԍ��P, �e�`�w�ԍ��Q, �e�`�w�ԍ��R \n"
						+ ", �Z���b�c, �Z���P, �Z���Q, �Z���R \n"
						+ ", ���O�P, ���O�Q, ���O�R \n"
						+ ", �X�֔ԍ� \n"
						+ ", ���X�b�c, ���X��, ����v \n"
						+ ", �ב��l�b�c, �ב��l������ \n"
						+ ", �W��X�b�c, ���X�b�c, ���X�� \n"
						+ ", ���Ӑ�b�c, ���ۂb�c, ���ۖ� \n"
						+ ", ��, �ː�, �d��, ���j�b�g \n"
						+ ", �w���, �w����敪 \n"
						+ ", �A���w���b�c�P, �A���w���P \n"
						+ ", �A���w���b�c�Q, �A���w���Q \n"
						+ ", �i���L���P, �i���L���Q, �i���L���R \n"
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
						+ ", �i���L���S, �i���L���T, �i���L���U \n"
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
						+ ", �����敪, �ی����z, �^��, ���p, ������ \n"
						+ ", �d���b�c, �����ԍ�, �����敪 \n"
						+ ", ����󔭍s�ςe�f, �o�׍ςe�f, ���M�ςe�f, �ꊇ�o�ׂe�f \n"
						+ ", ���, �ڍ׏�� \n"
						// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� START
						+ ", �����O�Q \n"
						// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� END
						+ ", �폜�e�f, �o�^����, �o�^�o�f, �o�^�� \n"
						+ ", �X�V����, �X�V�o�f, �X�V�� \n"
						+ ") \n"
						+ "VALUES ('" + sData[0] +"','" + sData[1] +"','" + s���t +"'," + i�Ǘ��m�n +",'" + sData[2] +"', \n"
						+         "'" + sData[3] +"','" + sData[4] +"', \n"
						+         "'" + sData[5] +"','" + sData[6] +"','" + sData[7] +"',' ',' ',' ', \n"
						+         "'" + s�Z���b�c +"','" + sData[8] +"','" + sData[9] +"','" + sData[10] +"', \n"
						+         "'" + sData[11] +"','" + sData[12] +"',' ', \n"
						+         "'" + sData[13] + sData[14] +"', \n"
						+         "'" + s���X�b�c +"','" + s���X�� + "','" + s����v +"', \n"        //���X�b�c�@���X���@����v
						+         "'" + sData[15] +"','" + sData[37] +"', \n"						  // �ב��l�b�c  �ב��l������
						+         "'" + s�W��X�b�c + "','" + s���X�b�c + "','" + s���X�� + "', \n"  //�W��X�b�c�@���X�b�c�@���X��
						+         "'" + sData[16] +"','" + sData[17] +"','" + sData[18] +"', \n"
						+         "" + sData[19] +"," + sData[38] +"," + sData[20] +",0, \n"
						+         "'" + sData[21] +"','" + sData[41] +"', \n"
						+         "'" + sData[39] +"','" + sData[22] +"', \n"
						+         "'" + sData[40] +"','" + sData[23] +"', \n"
						+         "'" + sData[24] +"','" + sData[25] +"','" + sData[26] +"', \n"
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
						+         "'" + s�i���L���S +"','"+ s�i���L���T +"','"+ s�i���L���U +"', \n"
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
						+         "'" + sData[27] +"'," + sData[28] +",0,0,0, \n"  //�^���@���p�@������
						+         "'" + s�d���b�c + "',' ',' ',"  //  �d���b�c  �����ԍ�  �����敪
						+         "'" + sData[29] +"','" + sData[30] +"', '0', '" + sData[31] +"', \n"  //   ���M�ςe�f
						+         "'01','  ', \n"        //��ԁ@�ڍ׏��
						// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� START
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� START
						//						+         "'" + s�ː��d�� + "', \n" // �����O�Q
						+         "' ', \n" // �����O�Q
						// MOD 2011.07.14 ���s�j���� �L���s�̒ǉ� END
						// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� END
						+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[32] +"','" + sData[33] +"', \n"
						+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[32] +"','" + sData[33] +"')";
					logWriter(sUser, INF, "�o�דo�^["+sData[1]+"]["+s���t+"]["+i�Ǘ��m�n+"]");

					iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					tran.Commit();
					sRet[0] = "����I��";
					sRet[1] = s���t;
					sRet[2] = i�Ǘ��m�n.ToString();
				}

			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
				if(ex.Number == 1438)
				{ // ORA-01438: value larger than specified precision allows for this column
					//					if(i�Ǘ��m�n > 9999){
					sRet[0] = "�P���ň�����o�א��i9999���j���z���܂����B";
					//					}
				}
			}
			catch (Exception ex)
			{
				tran.Rollback();
				string sErr = ex.Message.Substring(0,9);
				if(sErr == "ORA-00001")
					sRet[0] = "����̃R�[�h�����ɑ��̒[�����o�^����Ă��܂��B\r\n�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
				else
					sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�o�ד��擾
		 * �����F����b�c�A����b�c
		 * �ߒl�F�X�e�[�^�X�A�o�ד�
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(2246):
		*/
		private String[] Get_bumonsyukka(string[] sUser, OracleConnection conn2, string sKcode, string sBcode)
		{
			string[] sRet = new string[2];

			string cmdQuery = "SELECT �o�ד� \n"
				+ " FROM �b�l�O�Q���� \n"
				+ " WHERE ����b�c   = '" + sKcode + "' \n"
				+ "   AND ����b�c   = '" + sBcode + "' \n"
				+ "   AND �폜�e�f   = '0' \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			bool bRead = reader.Read();
			if(bRead == true)
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0).Trim();
			}
			else
			{
				sRet[0] = "�o�ד��G���[";
				sRet[1] = "0";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
	
		}

		/*********************************************************************
		 * �d���b�c�擾
		 * �����F����b�c�A����b�c�A�c�a�ڑ��A���X�A���X
		 * �ߒl�F�X�e�[�^�X�A�d���b�c
		 *
		 *********************************************************************/
		private static string GET_SIWAKE_SELECT
			= "SELECT �d���b�c \n"
			+ " FROM �b�l�P�V�d�� \n"
			;

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(2206):
		*/
		private String[] Get_siwake(string[] sUser, OracleConnection conn2, string sHatuCd, string sTyakuCd)
		{
			//			logWriter(sUser, INF, "�d���b�c�擾�J�n");

			string[] sRet = new string[2];

			string cmdQuery = GET_SIWAKE_SELECT
				+ " WHERE ���X���b�c = '" + sHatuCd + "' \n"
				+ " AND ���X���b�c = '" + sTyakuCd + "' \n"
				+ " AND �폜�e�f = '0' \n"
				;

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			if(reader.Read())
			{
				sRet[0] = " ";
				sRet[1] = reader.GetString(0);
			}
			else
			{
				sRet[0] = "�d���b�c�����߂��܂���ł���";
				sRet[1] = " ";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * �Z���}�X�^�ꗗ�擾
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i�X�֔ԍ��A�s���{�����j...
		 *
		 * �Q�ƌ��F�Z������.cs
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(3993):
		*/
		[WebMethod]
		public String[] Get_byPostcodeM(string[] sUser, string s�X�֔ԍ�)
		{
			logWriter(sUser, INF, "�Z���}�X�^�ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT '|' || TRIM(CM13.�X�֔ԍ�) || '|' "
					+ "|| TRIM(CM13.�s���{����) || TRIM(CM13.�s�撬����) || TRIM(CM13.�厚�ʏ̖�) || '|' "			//�Z��
					+ "|| TRIM(CM13.�s���{���b�c) || TRIM(CM13.�s�撬���b�c) || TRIM(CM13.�厚�ʏ̂b�c) || '|' "	//�Z���b�c
					+ "|| NVL(CM10.�X����, ' ') || '|' \n"
					+  " FROM �b�l�P�R�Z���i CM13 \n" // ���q�^���Ή�
					+  " LEFT JOIN �b�l�P�O�X�� CM10 \n"
					+    " ON CM13.�X���b�c = CM10.�X���b�c "
					+    "AND CM10.�폜�e�f = '0' \n";
				if(s�X�֔ԍ�.Length == 7)
				{
					cmdQuery += " WHERE CM13.�X�֔ԍ� = '" + s�X�֔ԍ� + "' \n";
				}
				else
				{
					cmdQuery +=  " WHERE CM13.�X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' \n";
				}
				cmdQuery +=    " AND CM13.�폜�e�f = '0' \n"
					+  " ORDER BY �厚�ʏ̂b�c \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				while (reader.Read())
				{
					sList.Add(reader.GetString(0).Trim());
				}

				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �Z���}�X�^�ꗗ�擾(�s)
		 * �����F�s���{���b�c�A�s�撬���b�c
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i�X�֔ԍ��A�厚�ʏ̖��j...
		 *
		 *********************************************************************/
		private static string GET_BYKENSHIM_SELECT
			= "SELECT '|' || TRIM(MAX(CM13.�X�֔ԍ�)) || '|' "
			+ "|| TRIM(MAX(CM13.�厚�ʏ̖�)) || '|' "
			+ "|| TRIM(MAX(CM13.�s���{���b�c))"
			+ "|| TRIM(MAX(CM13.�s�撬���b�c))"
			+ "|| TRIM(MAX(CM13.�厚�ʏ̂b�c)) || '|' "
			+ "|| MIN(NVL(CM10.�X����, ' ')) || '|' \n"
			+  " FROM �b�l�P�R�Z���i CM13 \n" // ���q�^���Ή�
			+  " LEFT JOIN �b�l�P�O�X�� CM10 \n"
			+    " ON CM13.�X���b�c = CM10.�X���b�c "
			+    "AND CM10.�폜�e�f = '0' \n"
			;
		private static string GET_BYKENSHIM_WHERE
			= " AND CM13.�폜�e�f = '0' \n"
			+ " GROUP BY CM13.�厚�ʏ̂b�c \n"
			+ " ORDER BY CM13.�厚�ʏ̂b�c \n"
			;
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(3884):
		*/
		[WebMethod]
		public String[] Get_byKenShiM(string[] sUser, string s�s���{���b�c, string s�s�撬���b�c)
		{
			logWriter(sUser, INF, "�Z���}�X�^�ꗗ�擾(�s)�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			string cmdQuery = "";
			try
			{
				cmdQuery
					= GET_BYKENSHIM_SELECT
					+ " WHERE CM13.�s���{���b�c = '" + s�s���{���b�c + "' \n"
					+   " AND CM13.�s�撬���b�c = '" + s�s�撬���b�c + "' \n"
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
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �厚�ʏ̖��ꗗ�̎擾
		 * �����F�s���{���b�c�A�s�撬���b�c
		 * �ߒl�F�X�e�[�^�X�A�厚�ʏ̖��ꗗ
		 *
		 *********************************************************************/
		private static string GET_BYKENSHI_SELECT
			= "SELECT MAX(�X�֔ԍ�), �厚�ʏ̖�, �厚�ʏ̃J�i��, MAX(�s���{���b�c), MAX(�s�撬���b�c), �厚�ʏ̂b�c, MAX(�X���b�c) \n"
			+   "FROM �b�l�P�R�Z���i \n"; // ���q�^���Ή�

		private static string GET_BYKENSHI_ORDER
			=    "AND �폜�e�f = '0' \n"
			+  "GROUP BY �厚�ʏ̂b�c,�厚�ʏ̖�,�厚�ʏ̃J�i�� \n"
			+  "ORDER BY �厚�ʏ̃J�i��, �厚�ʏ̂b�c \n"
			;

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2address\Service1.asmx.cs(286):
		*/
		[WebMethod]
		public String[] Get_byKenShi(string[] sUser, string s�s���{���b�c, string s�s�撬���b�c)
		{
			logWriter(sUser, INF, "�厚�ʏ̖��ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKENSHI_SELECT);
				sbQuery.Append(" WHERE �s���{���b�c = '" + s�s���{���b�c + "' \n");
				sbQuery.Append("   AND �s�撬���b�c = '" + s�s�撬���b�c + "' \n");
				sbQuery.Append(GET_BYKENSHI_ORDER);
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));
					sbRet.Append("|" + reader.GetString(1).Trim());
					sbRet.Append("|D" + "|");
					sbRet.Append(reader.GetString(2).Trim());		// �厚�ʏ̃J�i��
					sbRet.Append("|" + reader.GetString(3).Trim());	// �s���{���b�c
					sbRet.Append(reader.GetString(4).Trim());		// �s�撬���b�c
					sbRet.Append(reader.GetString(5).Trim());		// �厚�ʏ̂b�c
					sbRet.Append("|" + reader.GetString(6).Trim());	// �X���b�c

					sList.Add(sbRet);
				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �Z���ꗗ�̎擾
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�Z���ꗗ
		 *
		 *********************************************************************/
		private static string GET_BYPOSTCODE_SELECT
			= "SELECT �X�֔ԍ�, �s���{����, �s�撬����, �厚�ʏ̖�, �厚�ʏ̃J�i��, �s���{���b�c, �s�撬���b�c, �厚�ʏ̂b�c, �X���b�c \n"
			+  " FROM �b�l�P�R�Z���i \n"; // ���q�^���Ή�

		private static string GET_BYPOSTCODE_ORDER
			=    "AND �폜�e�f = '0' \n"
			+  "ORDER BY �X�֔ԍ�, �厚�ʏ̃J�i�� \n";

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2address\Service1.asmx.cs(415):
		*/
		[WebMethod]
		public String[] Get_byPostcode(string[] sUser, string s�X�֔ԍ�)
		{
			logWriter(sUser, INF, "�Z���ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYPOSTCODE_SELECT);
				if(s�X�֔ԍ�.Length == 7)
				{
					sbQuery.Append(" WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' ");
				}
				else
				{
					sbQuery.Append(" WHERE �X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' ");
				}
				sbQuery.Append(GET_BYPOSTCODE_ORDER);

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));		// �X�֔ԍ�
					sbRet.Append("|" + reader.GetString(1).Trim());	// �s���{����
					sbRet.Append(reader.GetString(2).Trim());		// �s�撬����
					sbRet.Append(reader.GetString(3).Trim());		// �厚�ʏ̖�
					sbRet.Append("|D" + "|");
					sbRet.Append(reader.GetString(4).Trim());		// �厚�ʏ̃J�i��
					sbRet.Append("|" + reader.GetString(5).Trim());	// �s���{���b�c
					sbRet.Append(reader.GetString(6).Trim());		// �s�撬���b�c
					sbRet.Append(reader.GetString(7).Trim());		// �厚�ʏ̂b�c
					sbRet.Append("|" + reader.GetString(8).Trim());	// �X���b�c
					sList.Add(sbRet);

				}
				disposeReader(reader);
				reader = null;
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �\�����ǉ�
		 * �����F�Ǘ��ԍ��A�����...
		 * �ߒl�F�X�e�[�^�X�A�X�V�����A�Ǘ��ԍ�
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(5972):
		*/
		[WebMethod]
		public string[] Ins_Mosikomi(string[] sUser, string[] sData)
		{
			//�Ǘ��ԍ��̎擾
			string[] sKey   = {" ", sData[42]};	//�X���b�c�A�X�V��
			string[] sKanri = Get_KaniSaiban(sUser, sKey);
			if(sKanri[0].Length > 4)
			{
				return sKanri;
			}
			sData[0] = sKanri[1];

			logWriter(sUser, INF, "�\�����ǉ��J�n");

			OracleConnection conn2 = null;

			string s�X�V���� = System.DateTime.Now.ToString("yyyyMMddHHmmss");
			string[] sRet = new string[3]{"", s�X�V����, sData[0]};

			string s�X�V�o�f = "�\���o�^";
			if(sData.Length > 43)
				s�X�V�o�f = sData[43];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			string cmdQuery = "";
			try
			{
				cmdQuery
					= "SELECT �폜�e�f \n"
					+   "FROM �r�l�O�T����\�� \n"
					+  "WHERE �Ǘ��ԍ� = " + sData[0] + " \n"
					+    "FOR UPDATE \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				int iCnt = 1;
				string s�폜�e�f = "";
				if(reader.Read())
				{
					s�폜�e�f = reader.GetString(0);
					iCnt++;
				}

				if(iCnt == 1)
				{
					//�ǉ�
					cmdQuery
						= "INSERT INTO �r�l�O�T����\�� \n"
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
						+         "," + s�X�V����
						+         ",'" + s�X�V�o�f + "' "
						+         ",'" + sData[42] + "' \n"
						+         "," + s�X�V����
						+         ",'" + s�X�V�o�f + "' "
						+         ",'" + sData[42] + "' \n"
						+ " ) \n";

					CmdUpdate(sUser, conn2, cmdQuery);

					tran.Commit();
					sRet[0] = "����I��";


				}
				else
				{
					//�ǉ��X�V
					if (s�폜�e�f.Equals("1"))
					{
						cmdQuery
							= "UPDATE �r�l�O�T����\�� \n"
							+   " SET �X���b�c = '" + sData[1] + "' \n"
							+       ",�\���҃J�i = '" + sData[2] + "' \n"
							+       ",�\���Җ� = '" + sData[3] + "' \n"
							+       ",�\���җX�֔ԍ� = '" + sData[4] + "' \n"
							+       ",�\���Ҍ��b�c = '" + sData[5] + "' \n"
							+       ",�\���ҏZ���P = '" + sData[6] + "' \n"
							+       ",�\���ҏZ���Q = '" + sData[7] + "' \n"
							+       ",�\���ғd�b�P = '" + sData[8] + "' \n"
							+       ",�\���ғd�b�Q = '" + sData[9] + "' \n"
							+       ",�\���ғd�b�R = '" + sData[10] + "' \n"
							+       ",�\���ғd�b = '" + sData[11] + "' \n"
							+       ",�\���҂e�`�w�P = '" + sData[12] + "' \n"
							+       ",�\���҂e�`�w�Q = '" + sData[13] + "' \n"
							+       ",�\���҂e�`�w�R = '" + sData[14] + "' \n"
							+       ",�ݒu�ꏊ�敪 = '" + sData[15] + "' \n"
							+       ",�ݒu�ꏊ�J�i = '" + sData[16] + "' \n"
							+       ",�ݒu�ꏊ�� = '" + sData[17] + "' \n"
							+       ",�ݒu�ꏊ�X�֔ԍ� = '" + sData[18] + "' \n"
							+       ",�ݒu�ꏊ���b�c = '" + sData[19] + "' \n"
							+       ",�ݒu�ꏊ�Z���P = '" + sData[20] + "' \n"
							+       ",�ݒu�ꏊ�Z���Q = '" + sData[21] + "' \n"
							+       ",�ݒu�ꏊ�d�b�P = '" + sData[22] + "' \n"
							+       ",�ݒu�ꏊ�d�b�Q = '" + sData[23] + "' \n"
							+       ",�ݒu�ꏊ�d�b�R = '" + sData[24] + "' \n"
							+       ",�ݒu�ꏊ�e�`�w�P = '" + sData[25] + "' \n"
							+       ",�ݒu�ꏊ�e�`�w�Q = '" + sData[26] + "' \n"
							+       ",�ݒu�ꏊ�e�`�w�R = '" + sData[27] + "' \n"
							+       ",�ݒu�ꏊ�S���Җ� = '" + sData[28] + "' \n"
							+       ",�ݒu�ꏊ��E�� = '" + sData[29] + "' \n"
							+       ",�ݒu�ꏊ�g�p�� =  " + sData[30] + "  \n"
							+       ",����b�c = '" + sData[31] + "' \n"
							+       ",�g�p�J�n�� = '" + sData[32] + "' \n"
							+       ",����b�c = '" + sData[33] + "' \n"
							+       ",���喼 = '" + sData[34] + "' \n"
							+       ",�T�[�}���䐔 =  " + sData[35] + "  \n"
							+       ",���p�҂b�c = '" + sData[36] + "' \n"
							+       ",���p�Җ� = '" + sData[37] + "' \n"
							+       ",�p�X���[�h = '" + sData[38] + "' \n"
							+       ",���F��Ԃe�f = '" + sData[39] + "' \n"
							+       ",���� = '" + sData[40] + "' \n"
							+       ",�폜�e�f = '0' \n"
							+       ",�o�^���� = " + s�X�V���� + " \n"
							+       ",�o�^�o�f = '" + s�X�V�o�f + "' \n"
							+       ",�o�^�� = '" + sData[42] + "' \n"
							+       ",�X�V���� = " + s�X�V���� + " \n"
							+       ",�X�V�o�f = '" + s�X�V�o�f + "' \n"
							+       ",�X�V�� = '" + sData[42] + "' \n"
							+ " WHERE �Ǘ��ԍ� = '" + sData[0] + "' \n";

						CmdUpdate(sUser, conn2, cmdQuery);

						string sRet���   = "";
						string sRet����   = "";
						string sRet���p�� = "";
						//���F��Ԃe�f��[3�F���F��]�̏ꍇ
						if(sData[39].Equals("3"))
						{
							sRet��� = Ins_Member2(sUser, conn2, sData, s�X�V����);
							if(sRet���.Length == 4)
							{
								//����}�X�^�ǉ�
								sRet���� = Ins_Section2(sUser, conn2, sData, s�X�V����);
								if(sRet����.Length == 4)
								{
									//���p�҃}�X�^�ǉ�
									sRet���p�� = Ins_User2(sUser, conn2, sData, s�X�V����);
								}
							}
						}
						if(sRet���.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "���q�l�F" + sRet���;
						}
						else if(sRet����.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "�Z�N�V�����F" + sRet����;
						}
						else if(sRet���p��.Length > 4)
						{
							tran.Rollback();
							sRet[0] = "���[�U�[�F" + sRet���p��;
						}
						else
						{
							tran.Commit();
							sRet[0] = "����I��";

						}
					}
					else
					{
						tran.Rollback();
						sRet[0] = "���ɓo�^����Ă��܂�";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�ǉ��Q
		 * �����F����b�c�A�����...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(6792):
		*/
		private string Ins_Member2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			//����}�X�^�ǉ�
			string[] sKey = new string[4]{
											 sData[31],	//����b�c
											 sData[3],	//�\���Җ�
											 sData[32],	//�g�p�J�n��
											 sData[42]	//�o�^�ҁA�X�V��
										 };

			string sRet = "";

			string cmdQuery = "";
			cmdQuery
				= "SELECT �폜�e�f \n"
				+   "FROM �b�l�O�P��� \n"
				+  "WHERE ����b�c = '" + sKey[0] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string s�폜�e�f = "";
			while (reader.Read())
			{
				s�폜�e�f = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//�ǉ�
				cmdQuery
					= "INSERT INTO �b�l�O�P��� \n"
					+ " VALUES ('" + sKey[0] + "' "		//����b�c
					+         ",'" + sKey[1] + "' "		//�����
					+         ",'" + sKey[2] + "' "		//�g�p�J�n��
					+         ",'99999999' "			//�g�p�I����
					+         ",'3' \n"					//�Ǘ��ҋ敪 // 3:���q���
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
					+         ",'����o�^' "
					+         ",'" + sKey[3] + "' \n"
					+         "," + sUpdateTime
					+         ",'����o�^' "
					+         ",'" + sKey[3] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);

				sRet = "����I��";
			}
			else
			{
				//�ǉ��X�V
				if (s�폜�e�f.Equals("1"))
				{
					cmdQuery
						= "UPDATE �b�l�O�P��� \n"
						+   " SET ����� = '" + sKey[1] + "' \n"
						+       ",�g�p�J�n�� = '" + sKey[2] + "' \n"
						+       ",�g�p�I���� = '99999999' \n"
						+       ",�Ǘ��ҋ敪 = '3' \n" // 3:���q���
						// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
						+       ",�L���A�g�e�f = '0' \n"
						+       ",�ۗ�����e�f = '0' \n"
						// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						+       ",�폜�e�f = '0' \n"
						+       ",�o�^���� = " + sUpdateTime
						+       ",�o�^�o�f = '����o�^' "
						+       ",�o�^�� = '" + sKey[3] + "' \n"
						+       ",�X�V���� = " + sUpdateTime
						+       ",�X�V�o�f = '����o�^' "
						+       ",�X�V�� = '" + sKey[3] + "' \n"
						+ " WHERE ����b�c = '" + sKey[0] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);

					sRet = "����I��";
				}
				else
				{
					sRet = "���ɓo�^����Ă��܂�";
				}
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * �Ǘ��ԍ��̍̔�
		 * �����F����b�c�A����b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(6655):
		*/
		[WebMethod]
		public String[] Get_KaniSaiban(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "�Ǘ��ԍ��̎擾�J�n");
			
			OracleConnection conn2 = null;
			string[] sRet = new string[2]{"",""};
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				decimal i�J�����g�ԍ� = 0;
				decimal i�J�n�ԍ�     = 0;
				decimal i�I���ԍ�     = 0;

				string cmdQuery
					= "SELECT �J�����g�ԍ�, �J�n�ԍ�, �I���ԍ� \n"
					+ " FROM �b�l�P�U�X���̔ԊǗ� \n"
					+ " WHERE �̔ԋ敪 = '01' \n"
					+ " AND �X���b�c = '" + sKey[0] + "' \n"
					+ " FOR UPDATE \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				string updQuery = "";
				if(reader.Read())
				{
					i�J�����g�ԍ� = reader.GetDecimal(0);
					i�J�n�ԍ�     = reader.GetDecimal(1);
					i�I���ԍ�     = reader.GetDecimal(2);

					if(i�J�����g�ԍ� < i�I���ԍ�)
					{
						i�J�����g�ԍ�++;
					}
					else
					{
						i�J�����g�ԍ� = i�J�n�ԍ�;
					}
					sRet[1] = i�J�����g�ԍ�.ToString("0000000");

					updQuery 
						= "UPDATE �b�l�P�U�X���̔ԊǗ� SET \n"
						+ "  �J�����g�ԍ� = " + i�J�����g�ԍ� + " \n"
						+ ", �J�n�ԍ� = " + i�J�n�ԍ� + " \n"
						+ ", �I���ԍ� = " + i�I���ԍ� + " \n"
						+ ", �X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ ", �X�V�o�f = '����\��' \n"
						+ ", �X�V�� = '" + sKey[1] + "' \n"
						+ " WHERE �̔ԋ敪 = '01' \n"
						+ " AND �X���b�c = '" + sKey[0] + "' \n"
						+ " AND �폜�e�f = '0' \n";
				}
				else
				{
					i�J�����g�ԍ� = 5005001;
					i�J�n�ԍ�     = 1000001;
					i�I���ԍ�     = 9999999;
					sRet[1] = i�J�����g�ԍ�.ToString("0000000");

					// �����̔Ԃ̒ǉ�
					updQuery 
						= "INSERT INTO �b�l�P�U�X���̔ԊǗ� VALUES( \n"
						+ " '01' \n"
						+ ",'" + sKey[0] + "' \n"
						+ ", " + i�J�����g�ԍ� + " \n"
						+ ", " + i�J�n�ԍ� + " \n"
						+ ", " + i�I���ԍ� + " \n"
						+ ",'0' \n"
						+ ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
						+ ",'����\��' "
						+ ",'" + sKey[1] + "' \n"
						+ ", TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
						+ ",'����\��' "
						+ ",'" + sKey[1] + "' \n"
						+ ") \n";
				}
				CmdUpdate(sUser, conn2, updQuery);
				disposeReader(reader);
				reader = null;
				tran.Commit();
				sRet[0] = "����I��";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * ����}�X�^�ǉ��Q
		 * �����F����b�c�A����b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(6904):
		*/
		private string Ins_Section2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			string[] sKey = new string[10]{
											  sData[31],	//����b�c
											  sData[33],	//����b�c
											  sData[34],	//���喼
											  sData[18],	//�ݒu�ꏊ�X�֔ԍ�
											  sData[20],	//�ݒu�ꏊ�Z���P
											  sData[21],	//�ݒu�ꏊ�Z���Q
											  sData[35],	//�T�[�}���䐔
											  sData[42]	//�o�^�ҁA�X�V��
											  ,sData[30]	//�ݒu�ꏊ�g�p��
											  ,sData[0]	//�Ǘ��ԍ�
										  };
			string sRet = "";

			string cmdQuery = "";

			cmdQuery
				= "SELECT �폜�e�f \n"
				+   "FROM �b�l�O�Q���� \n"
				+  "WHERE ����b�c = '" + sKey[0] + "' \n"
				+    "AND ����b�c = '" + sKey[1] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string s�폜�e�f = "";
			while (reader.Read())
			{
				s�폜�e�f = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//�ǉ�
				cmdQuery
					= "INSERT INTO �b�l�O�Q���� \n"
					+         "(����b�c \n"
					+         ",����b�c \n"
					+         ",���喼 \n"
					+         ",�g�D�b�c \n"
					+         ",�o�͏� \n"
					+         ",�X�֔ԍ� \n"
					+         ",\"�W���[�i���m�n�o�^��\" \n"
					+         ",\"�W���[�i���m�n�Ǘ�\" \n"
					+         ",���^�m�n \n"
					+         ",�o�ד� \n"
					+         ",�ݒu��Z���P \n"
					+         ",�ݒu��Z���Q \n"
					+         ",�T�[�}���䐔 \n"
					+         ",�폜�e�f \n"
					+         ",�o�^���� \n"
					+         ",�o�^�o�f \n"
					+         ",�o�^�� \n"
					+         ",�X�V���� \n"
					+         ",�X�V�o�f \n"
					+         ",�X�V�� \n"
					+         ") \n"
					+ " VALUES ('" + sKey[0] + "' "				//����b�c
					+         ",'" + sKey[1] + "' "				//����b�c
					+         ",'" + sKey[2] + "' "				//���喼
					+         ",' ' "							//�g�D�b�c
					+         ", 0 \n"							//�o�͏�
					+         ",'" + sKey[3] + "' "				//�X�֔ԍ�
					+         ",TO_CHAR(SYSDATE,'YYYYMMDD') "	//�W���[�i���m�n�o�^��
					+         ", 0 "							//�W���[�i���Ǘ��m�n
					+         ", 0 "							//���^�m�n
					+         ",TO_CHAR(SYSDATE,'YYYYMMDD') \n"	//�o�ד�
					+         ",'" + sKey[4] + "' "				//�ݒu��Z���P
					+         ",'" + sKey[5] + "' "				//�ݒu��Z���Q
					+         ", " + sKey[6] + " \n"			//�T�[�}���䐔
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'����o�^' "
					+         ",'" + sKey[7] + "' \n"
					+         "," + sUpdateTime
					+         ",'����o�^' "
					+         ",'" + sKey[7] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);

				cmdQuery
					= "INSERT INTO �b�l�O�U����g�� \n"
					+         "(����b�c \n"
					+         ",����b�c \n"
					+         ",�g�p�� \n"
					+         ",����\���Ǘ��ԍ� \n"
					+         ",�폜�e�f \n"
					+         ",�o�^���� \n"
					+         ",�o�^�o�f \n"
					+         ",�o�^�� \n"
					+         ",�X�V���� \n"
					+         ",�X�V�o�f \n"
					+         ",�X�V�� \n"
					+         ") \n"
					+ " VALUES ('" + sKey[0] + "' "				//����b�c
					+         ",'" + sKey[1] + "' "				//����b�c
					+         ", " + sKey[8] + " \n"			//�g�p��
					+         ", " + sKey[9] + " \n"			//����\���Ǘ��ԍ�
					+         ",'0' \n"
					+         "," + sUpdateTime
					+         ",'����o�^' "
					+         ",'" + sKey[7] + "' \n"
					+         "," + sUpdateTime
					+         ",'����o�^' "
					+         ",'" + sKey[7] + "' \n"
					+ " ) \n";
				CmdUpdate(sUser, conn2, cmdQuery);

				sRet = "����I��";
			}
			else
			{
				//�ǉ��X�V
				if (s�폜�e�f.Equals("1"))
				{
					cmdQuery
						= "UPDATE �b�l�O�Q���� \n"
						+   " SET ���喼 = '" + sKey[2] + "' \n"
						+       ",�g�D�b�c = ' ' \n"
						+       ",�o�͏� = 0 \n"
						+       ",�X�֔ԍ� = '" + sKey[3] + "' \n"
						+       ",�W���[�i���m�n�o�^�� = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+       ",�W���[�i���m�n�Ǘ� = 0 \n"
						+       ",���^�m�n = 0 \n"
						+       ",�o�ד� = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
						+       ",�ݒu��Z���P = '" + sKey[4] + "' \n"
						+       ",�ݒu��Z���Q = '" + sKey[5] + "' \n"
						+       ",�T�[�}���䐔 =  " + sKey[6] + " \n"
						+       ",�폜�e�f = '0' \n"
						+       ",�o�^���� = " + sUpdateTime
						+       ",�o�^�o�f = '����o�^' "
						+       ",�o�^�� = '" + sKey[7] + "' \n"
						+       ",�X�V���� = " + sUpdateTime
						+       ",�X�V�o�f = '����o�^' "
						+       ",�X�V�� = '" + sKey[7] + "'\n"
						+ " WHERE ����b�c = '" + sKey[0] + "' \n"
						+   " AND ����b�c = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					cmdQuery
						= "UPDATE �b�l�O�U����g�� SET \n"
						+       " �g�p�� = " + sKey[8] + " \n"
						+       ",����\���Ǘ��ԍ� = " + sKey[9] + " \n"
						+       ",�폜�e�f = '0' \n"
						+       ",�o�^���� = " + sUpdateTime
						+       ",�o�^�o�f = '����o�^' "
						+       ",�o�^�� = '" + sKey[7] + "' \n"
						+       ",�X�V���� = " + sUpdateTime
						+       ",�X�V�o�f = '����o�^' "
						+       ",�X�V�� = '" + sKey[7] + "'\n"
						+ " WHERE ����b�c = '" + sKey[0] + "' \n"
						+   " AND ����b�c = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "����I��";
				}
				else
				{
					sRet = "���ɓo�^����Ă��܂�";
				}
			}
			disposeReader(reader);
			reader = null;

			//�G���[���́A�I��
			if (!sRet.Equals("����I��")) return sRet;

			logWriter(sUser, INF, "�L���̏����f�[�^�o�^�J�n");

			//�L���̏����f�[�^�̌���
			cmdQuery
				= "SELECT �L���b�c \n"
				+      ", �L�� \n"
				+   "FROM �r�l�O�R�L�� \n"
				+  "WHERE ����b�c = 'default' \n"
				+    "AND ����b�c = '0000' \n"
				+    "AND �폜�e�f = '0' \n";

			OracleDataReader readerDef = CmdSelect(sUser, conn2, cmdQuery);
			string s�����L���b�c = "";
			string s�����L��     = "";
			while (readerDef.Read())
			{
				s�����L���b�c = readerDef.GetString(0);
				s�����L��     = readerDef.GetString(1);

				//�L���̌���
				cmdQuery
					= "SELECT �L���b�c \n"
					+   "FROM �r�l�O�R�L�� \n"
					+  "WHERE ����b�c = '" + sKey[0] + "' \n"
					+    "AND ����b�c = '" + sKey[1] + "' \n"
					+    "AND �L���b�c = '" + s�����L���b�c + "' \n"
					+    "FOR UPDATE \n";

				OracleDataReader readerNote = CmdSelect(sUser, conn2, cmdQuery);
				if (readerNote.Read())
				{
					//���ɋL��������ꍇ�͐V�K�X�V
					cmdQuery
						= "UPDATE �r�l�O�R�L�� \n"
						+   " SET �L�� = '" + s�����L�� + "' \n"
						+       ",�폜�e�f = '0' \n"
						+       ",�o�^���� = " + sUpdateTime
						+       ",�o�^�o�f = '�����L��' \n"
						+       ",�o�^�� = '" + sKey[7] + "' \n"
						+       ",�X�V���� = " + sUpdateTime
						+       ",�X�V�o�f = '�����L��' \n"
						+       ",�X�V�� = '" + sKey[7] + "' \n"
						+ " WHERE ����b�c = '" + sKey[0] + "' \n"
						+   " AND ����b�c = '" + sKey[1] + "' \n"
						+   " AND �L���b�c = '" + s�����L���b�c + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "����I��";
				}
				else
				{
					//�V�K�ǉ�
					cmdQuery
						= "INSERT INTO �r�l�O�R�L�� \n"
						+ " VALUES ('" + sKey[0] + "' " 
						+         ",'" + sKey[1] + "' "
						+         ",'" + s�����L���b�c + "' "
						+         ",'" + s�����L�� + "' \n"
						+         ",'0' \n"
						+         "," + sUpdateTime
						+         ",'�����L��' "
						+         ",'" + sKey[7] + "' \n"
						+         "," + sUpdateTime
						+         ",'�����L��' "
						+         ",'" + sKey[7] + "' \n"
						+ " ) \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "����I��";
				}
				disposeReader(readerNote);
				readerNote = null;
			}
			disposeReader(readerDef);
			readerDef = null;

			return sRet;
		}

		/*********************************************************************
		 * ���p�҃}�X�^�ǉ��Q
		 * �����F����b�c�A���p�҂b�c�A���p�Җ�
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(7197):
		*/
		private string Ins_User2(string[] sUser, OracleConnection conn2, 
			string[] sData, string sUpdateTime)
		{
			string[] sKey = new string[6]{
											 sData[31],	//����b�c
											 sData[36],	//���p�҂b�c
											 sData[38],	//�p�X���[�h
											 sData[37],	//���p�Җ�
											 sData[33],	//����b�c
											 sData[42]	//�o�^�ҁA�X�V��
										 };
			string sRet = "";

			string cmdQuery = "";

			cmdQuery
				= "SELECT �폜�e�f \n"
				+   "FROM �b�l�O�S���p�� \n"
				+  "WHERE ����b�c = '" + sKey[0] + "' \n"
				+    "AND ���p�҂b�c = '" + sKey[1] + "' \n"
				+    "FOR UPDATE \n";

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
			int iCnt = 1;
			string s�폜�e�f = "";
			while (reader.Read())
			{
				s�폜�e�f = reader.GetString(0);
				iCnt++;
			}
			if(iCnt == 1)
			{
				//�ǉ�
				cmdQuery
					= "INSERT INTO �b�l�O�S���p�� \n"
					+ " VALUES ('" + sKey[0] + "' "		//����b�c
					+         ",'" + sKey[1] + "' "		//���p�҂b�c
					+         ",'" + sKey[2] + "' "		//�p�X���[�h
					+         ",'" + sKey[3] + "' "		//���p�Җ�
					+         ",'" + sKey[4] + "' \n"	//����b�c
					+         ",' ' "					//�ב��l�b�c
					+         ",0 "						//�F�؃G���[��
					+         ",' ' "					//�����P
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
					+         ",'����o�^' "
					+         ",'" + sKey[5] + "' \n"
					+ " ) \n";

				CmdUpdate(sUser, conn2, cmdQuery);
				sRet = "����I��";
			}
			else
			{
				//�ǉ��X�V
				if (s�폜�e�f.Equals("1"))
				{
					cmdQuery
						= "UPDATE �b�l�O�S���p�� \n"
						+   " SET �p�X���[�h = '" + sKey[2] + "' \n"
						+       ",���p�Җ� = '" + sKey[3] + "' \n"
						+       ",����b�c = '" + sKey[4] + "' \n"
						+       ",�ב��l�b�c = ' ' \n"
						+       ",�F�؃G���[�� = 0 \n"
						+       ",�����P = ' ' \n"
						+       ",�폜�e�f = '0' \n"
						+       ",�o�^���� = " + sUpdateTime
						+       ",�o�^�o�f = '"+ sUpdateTime.Substring(0,8) +"' "
						+       ",�o�^�� = '" + sKey[5] + "' \n"
						+       ",�X�V���� = " + sUpdateTime
						+       ",�X�V�o�f = '����o�^' "
						+       ",�X�V�� = '" + sKey[5] + "' \n"
						+ " WHERE ����b�c = '" + sKey[0] + "' \n"
						+   " AND ���p�҂b�c = '" + sKey[1] + "' \n";

					CmdUpdate(sUser, conn2, cmdQuery);
					sRet = "����I��";
				}
				else
				{
					sRet = "���ɓo�^����Ă��܂�";
				}
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * �\�����X�V
		 * �����F�Ǘ��ԍ��A�����...
		 * �ߒl�F�X�e�[�^�X�A�X�V����
		 *
		 *********************************************************************/
		private static string UPD_MOSIKOMI_SELECT
			= "SELECT �Ǘ��ԍ� "
			+ ", �X���b�c "
			+ ", �\���҃J�i "
			+ ", �\���Җ� "
			+ ", �\���җX�֔ԍ� \n"
			+ ", �\���Ҍ��b�c "
			+ ", �\���ҏZ���P "
			+ ", �\���ҏZ���Q "
			+ ", �\���ғd�b�P "
			+ ", �\���ғd�b�Q \n"
			+ ", �\���ғd�b�R "
			+ ", �\���ғd�b "
			+ ", �\���҂e�`�w�P "
			+ ", �\���҂e�`�w�Q "
			+ ", �\���҂e�`�w�R \n"
			+ ", �ݒu�ꏊ�敪 "
			+ ", �ݒu�ꏊ�J�i "
			+ ", �ݒu�ꏊ�� "
			+ ", �ݒu�ꏊ�X�֔ԍ� "
			+ ", �ݒu�ꏊ���b�c \n"
			+ ", �ݒu�ꏊ�Z���P "
			+ ", �ݒu�ꏊ�Z���Q "
			+ ", �ݒu�ꏊ�d�b�P "
			+ ", �ݒu�ꏊ�d�b�Q "
			+ ", �ݒu�ꏊ�d�b�R \n"
			+ ", �ݒu�ꏊ�e�`�w�P "
			+ ", �ݒu�ꏊ�e�`�w�Q "
			+ ", �ݒu�ꏊ�e�`�w�R "
			+ ", �ݒu�ꏊ�S���Җ� "
			+ ", �ݒu�ꏊ��E�� \n"
			+ ", �ݒu�ꏊ�g�p�� "
			+ ", ����b�c "
			+ ", �g�p�J�n�� "
			+ ", ����b�c "
			+ ", ���喼 \n"
			+ ", \"�T�[�}���䐔\" "
			+ ", ���p�҂b�c "
			+ ", ���p�Җ� "
			+ ", \"�p�X���[�h\" "
			+ ", ���F��Ԃe�f \n"
			+ ", ���� "
			+ ", TO_CHAR(�X�V����) "
			+ ", �X�V�� \n"
			+ "FROM �r�l�O�T����\�� \n"
			+ "";

		private static string UPD_MOSIKOMI_DELETE
			= "UPDATE �r�l�O�T����\�� \n"
			+ "SET �폜�e�f = '1' \n"
			+ "";

		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2maintenance\Service1.asmx.cs(6303):
		*/
		[WebMethod]
		public string[] Upd_Mosikomi(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "�\�����X�V�J�n");

			OracleConnection conn2 = null;
			string s�X�V���� = System.DateTime.Now.ToString("yyyyMMddHHmmss");
			string[] sRet = new string[3]{"", s�X�V����, sData[0]};

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			string cmdQuery = "";

			try
			{
				bool bUpdState = false;

				//���F��Ԃe�f��[1�F�\����]�̏ꍇ�i����{�^���̎��j
				if(sData[39].Equals("1"))
				{
					string[] sRefData = new string[43];
					cmdQuery = UPD_MOSIKOMI_SELECT
						+ " WHERE �Ǘ��ԍ� = '" + sData[0] + "' \n"
						+ " AND �폜�e�f = '0' \n"
						+ " AND �X�V���� = " + sData[41] + " \n"
						+ " FOR UPDATE \n";

					OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read())
					{
						tran.Rollback();
						sRet[0] = "���̒[���ōX�V����Ă��܂�";
						logWriter(sUser, INF, sRet[0]);
						return sRet;
					}
					sRefData[0] = "";
					//�Ǘ��ԍ��̓_�~�[
					sRefData[1] = reader.GetString(1).Trim();
					sRefData[2] = reader.GetString(2).Trim();
					sRefData[3] = reader.GetString(3).Trim();
					sRefData[4] = reader.GetString(4).Trim();
					sRefData[5] = reader.GetString(5).Trim();	//�\���Ҍ��b�c
					sRefData[6] = reader.GetString(6).Trim();
					sRefData[7] = reader.GetString(7).Trim();
					sRefData[8] = reader.GetString(8).Trim();
					sRefData[9] = reader.GetString(9).Trim();
					sRefData[10] = reader.GetString(10).Trim();	//�\���ғd�b�R
					sRefData[11] = reader.GetString(11).Trim();
					sRefData[12] = reader.GetString(12).Trim();
					sRefData[13] = reader.GetString(13).Trim();
					sRefData[14] = reader.GetString(14).Trim();
					sRefData[15] = reader.GetString(15).Trim();	//�ݒu�ꏊ�敪
					sRefData[16] = reader.GetString(16).Trim();
					sRefData[17] = reader.GetString(17).Trim();
					sRefData[18] = reader.GetString(18).Trim();
					sRefData[19] = reader.GetString(19).Trim();
					sRefData[20] = reader.GetString(20).Trim();	//�ݒu�ꏊ�Z���P
					sRefData[21] = reader.GetString(21).Trim();
					sRefData[22] = reader.GetString(22).Trim();
					sRefData[23] = reader.GetString(23).Trim();
					sRefData[24] = reader.GetString(24).Trim();
					sRefData[25] = reader.GetString(25).Trim();	//�ݒu�ꏊ�e�`�w�P
					sRefData[26] = reader.GetString(26).Trim();
					sRefData[27] = reader.GetString(27).Trim();
					sRefData[28] = reader.GetString(28).Trim();
					sRefData[29] = reader.GetString(29).Trim();
					sRefData[30] = reader.GetDecimal(30).ToString().Trim();	//�ݒu�ꏊ�g�p��
					sRefData[31] = reader.GetString(31).Trim();
					sRefData[32] = reader.GetString(32).Trim();
					sRefData[33] = reader.GetString(33).Trim();
					sRefData[34] = reader.GetString(34).Trim();
					sRefData[35] = reader.GetDecimal(35).ToString().Trim();	//�T�[�}���䐔
					sRefData[36] = reader.GetString(36).Trim();
					sRefData[37] = reader.GetString(37).Trim();
					sRefData[38] = reader.GetString(38).Trim();
					sRefData[39] = reader.GetString(39).Trim();
					sRefData[40] = reader.GetString(40).Trim();	//����
					sRefData[41] = reader.GetString(41).Trim();
					sRefData[42] = reader.GetString(42).Trim();

					//���F��Ԃe�f�i_:�o�^���A1:�\�����A2:���ے��A3:���F�ρj��
					//�i1:�\������������2:���ے��̂��́j
					if(sRefData[39].Length > 0)
					{
						//�f�[�^�̍X�V�󋵂��`�F�b�N����
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
							//�f�[�^�폜
							cmdQuery = UPD_MOSIKOMI_DELETE
								+ ", �X�V�o�f = '�\���X�V' \n"
								+ ", �X�V��   = '" + sData[42] +"' \n"
								+ ", �X�V���� = "+ s�X�V���� + " \n"
								+ " WHERE �Ǘ��ԍ� = '" + sData[0] + "' \n"
								+ " AND �폜�e�f = '0' \n"
								+ " AND �X�V���� = " + sData[41] + " \n";

							if (CmdUpdate(sUser, conn2, cmdQuery) == 0)
							{
								tran.Rollback();
								sRet[0] = "���̒[���ōX�V����Ă��܂�";
							}
							else
							{
								tran.Commit();
								sRet[0] = "����I��";
							}
							logWriter(sUser, INF, sRet[0]);
							//�f�[�^���ύX����Ă���ꍇ�ɂ́A�V�����󒍂m�n�Ńf�[�^��ǉ�����
							//�ۗ��@�g�����U�N�V��������
							return Ins_Mosikomi(sUser, sData);
						}
					}
					disposeReader(reader);
					reader = null;
				}

				cmdQuery
					= "UPDATE �r�l�O�T����\�� \n"
					+   " SET �X���b�c = '" + sData[1] + "' \n"
					+       ",�\���҃J�i = '" + sData[2] + "' \n"
					+       ",�\���Җ� = '" + sData[3] + "' \n"
					+       ",�\���җX�֔ԍ� = '" + sData[4] + "' \n"
					+       ",�\���Ҍ��b�c = '" + sData[5] + "' \n"
					+       ",�\���ҏZ���P = '" + sData[6] + "' \n"
					+       ",�\���ҏZ���Q = '" + sData[7] + "' \n"
					+       ",�\���ғd�b�P = '" + sData[8] + "' \n"
					+       ",�\���ғd�b�Q = '" + sData[9] + "' \n"
					+       ",�\���ғd�b�R = '" + sData[10] + "' \n"
					+       ",�\���ғd�b = '" + sData[11] + "' \n"
					+       ",�\���҂e�`�w�P = '" + sData[12] + "' \n"
					+       ",�\���҂e�`�w�Q = '" + sData[13] + "' \n"
					+       ",�\���҂e�`�w�R = '" + sData[14] + "' \n"
					+       ",�ݒu�ꏊ�敪 = '" + sData[15] + "' \n"
					+       ",�ݒu�ꏊ�J�i = '" + sData[16] + "' \n"
					+       ",�ݒu�ꏊ�� = '" + sData[17] + "' \n"
					+       ",�ݒu�ꏊ�X�֔ԍ� = '" + sData[18] + "' \n"
					+       ",�ݒu�ꏊ���b�c = '" + sData[19] + "' \n"
					+       ",�ݒu�ꏊ�Z���P = '" + sData[20] + "' \n"
					+       ",�ݒu�ꏊ�Z���Q = '" + sData[21] + "' \n"
					+       ",�ݒu�ꏊ�d�b�P = '" + sData[22] + "' \n"
					+       ",�ݒu�ꏊ�d�b�Q = '" + sData[23] + "' \n"
					+       ",�ݒu�ꏊ�d�b�R = '" + sData[24] + "' \n"
					+       ",�ݒu�ꏊ�e�`�w�P = '" + sData[25] + "' \n"
					+       ",�ݒu�ꏊ�e�`�w�Q = '" + sData[26] + "' \n"
					+       ",�ݒu�ꏊ�e�`�w�R = '" + sData[27] + "' \n"
					+       ",�ݒu�ꏊ�S���Җ� = '" + sData[28] + "' \n"
					+       ",�ݒu�ꏊ��E�� = '" + sData[29] + "' \n"
					+       ",�ݒu�ꏊ�g�p�� =  " + sData[30] + "  \n"
					+       ",����b�c = '" + sData[31] + "' \n"
					+       ",�g�p�J�n�� = '" + sData[32] + "' \n"
					+       ",����b�c = '" + sData[33] + "' \n"
					+       ",���喼 = '" + sData[34] + "' \n"
					+       ",�T�[�}���䐔 =  " + sData[35] + "  \n"
					+       ",���p�҂b�c = '" + sData[36] + "' \n"
					+       ",���p�Җ� = '" + sData[37] + "' \n"
					+       ",�p�X���[�h = '" + sData[38] + "' \n"
					+       ",���F��Ԃe�f = '" + sData[39] + "' \n"
					+       ",���� = '" + sData[40] + "' \n"
					+       ",�X�V���� = " + s�X�V���� + " \n"
					+       ",�X�V�o�f = '�\���X�V' \n"
					+       ",�X�V�� = '" + sData[42] + "' \n"
					+ " WHERE �Ǘ��ԍ� = '" + sData[0] + "' \n"
					+   " AND �폜�e�f = '0' \n"
					+   " AND �X�V���� = " + sData[41] + " \n";

				if (CmdUpdate(sUser, conn2, cmdQuery) != 0)
				{
					string sRet���   = "";
					string sRet����   = "";
					string sRet���p�� = "";
					//���F��Ԃe�f��[3�F���F��]�̏ꍇ
					if(sData[39].Equals("3"))
					{
						sRet��� = Ins_Member2(sUser, conn2, sData, s�X�V����);
						if(sRet���.Length == 4)
						{
							//����}�X�^�ǉ�
							sRet���� = Ins_Section2(sUser, conn2, sData, s�X�V����);
							if(sRet����.Length == 4)
							{
								//���p�҃}�X�^�ǉ�
								sRet���p�� = Ins_User2(sUser, conn2, sData, s�X�V����);
							}
						}
					}
					if(sRet���.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "���q�l�F" + sRet���;
					}
					else if(sRet����.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "�Z�N�V�����F" + sRet����;
					}
					else if(sRet���p��.Length > 4)
					{
						tran.Rollback();
						sRet[0] = "���[�U�[�F" + sRet���p��;
					}
					else
					{
						tran.Commit();
						sRet[0] = "����I��";
						sRet[1] = s�X�V����;
					}
				}
				else
				{
					tran.Rollback();
					sRet[0] = "���̒[���ōX�V����Ă��܂�";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �����o�דo�^�p�Z���擾�R
		 * �@�@�r�l�O�Q�׎�l�A�b�l�P�S�X�֔ԍ��A�b�l�P�T���X��\���A�b�l�P�X�X�֏Z��
		 *     �̂R�}�X�^���g�p���Ē��X�R�[�h�����肷��B
		 * �����F����R�[�h�A����R�[�h�A�׎�l�R�[�h�A�X�֔ԍ��A�Z���A����
		 * �ߒl�F�X�e�[�^�X�A�X���b�c�A�X�����A�Z���b�c
		 *
		 * Create : 2008.06.12 kcl)�X�{
		 * �@�@�@�@�@�@Get_autoEntryPref �����ɍ쐬
		 * Modify : 2008.12.25 kcl)�X�{
		 *            �����Ɏ�����ǉ�
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2syukka\Service1.asmx.cs(5201):
		*/
		[WebMethod]
		public string [] Get_autoEntryPref3(string [] sUser, 
			string sKaiinCode, string sBumonCode, string sNiukeCode, 
			string sYuubin, string sJuusyo, string sShimei)
		{
			// ���O�o��
			logWriter(sUser, INF, "�Z���擾�R�J�n");

			OracleConnection conn2 = null;
			string [] sRet = new string [4];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				// �c�a�ڑ��Ɏ��s
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			try 
			{
				// ���X�R�[�h�̎擾
				string [] sResult = this.Get_tyakuten3(sUser, conn2, sKaiinCode, sBumonCode, sNiukeCode, sYuubin, sJuusyo, sShimei);

				if (sResult[0] == " ")
				{
					// �擾����
					sRet[1] = sResult[3];	// �Z���b�c
					sRet[2] = sResult[1];	// �X���b�c
					sRet[3] = sResult[2];	// �X����

					sRet[0] = "����I��";
				}
				else
				{
					// �擾���s
					sRet[0] = "�Y���f�[�^������܂���";
				}

				// ���O�o��
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				// Oracle �̃G���[
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				// ����ȊO�̃G���[
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				// �I������
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}
		// MOD 2011.06.06 ���s�j���� ���q�^���A�����i�R�[�h�����ǉ� START
		/*********************************************************************
		 * �A�����i�R�[�h����
		 * �����F����b�c�A�L��
		 * �ߒl�F�X�e�[�^�X
		 *       �A�����i������A�����i�R�[�h���������܂�
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2kiji\Service1.asmx.cs(571):
		*/
		[WebMethod]
		public String[] Get_kijiCD(string[] sUser, string sBcode, string sKname)
		{
			logWriter(sUser, INF, "�A�����i�R�[�h�����J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			try
			{
				StringBuilder sbQuery = new StringBuilder(1024);
				string sKcode = "";

				if(sBcode.Equals("100"))
				{
					if(sKname.StartsWith("���Ԏw��"))
					{
						if(sKname.EndsWith("�܂�"))
						{
							sKcode = "11X";
						}
						else if(sKname.EndsWith("�ȍ~"))
						{
							sKcode = "12X";
						}
					}
				}
				else if(sBcode.Equals("200"))
				{
					if(sKname.StartsWith("���Ԏw��"))
					{
						if(sKname.EndsWith("�܂�"))
						{
							sKcode = "21X";
						}
						else if(sKname.EndsWith("�ȍ~"))
						{
							sKcode = "22X";
						}
					}
				}

				sbQuery.Append( "SELECT �L���b�c" );
				sbQuery.Append(  " FROM �r�l�O�R�L�� \n" );
				sbQuery.Append( " WHERE ����b�c = 'Jyusoshohin' \n" ); // ���q�^���Ή�
				sbQuery.Append(   " AND ����b�c = '" + sBcode +"' \n" );
				if (sKcode.Length != 0)
				{
					sbQuery.Append(   " AND �L���b�c = '" + sKcode +"' \n" );
				}
				else
				{
					sbQuery.Append(   " AND �L��     = '" + sKname +"' \n" );
				}
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				if(reader.Read())
				{
					sRet[0] = "����I��";
					sRet[1] = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "�Y���f�[�^������܂���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		 * �L������f�[�^�擾
		 * �����F����b�c�A����b�c
		 * �ߒl�F�X�e�[�^�X�A�L���b�c�A�L��
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2print\Service1.asmx.cs(1520):
		*/
		[WebMethod]
		public ArrayList Get_NotePrintData(string[] sUser, string[] sKey)
		{
			logWriter(sUser, INF, "�L������f�[�^�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList alRet = new ArrayList();
			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				alRet.Add(sRet);
				return alRet;
			}

			try
			{
				//�A���w���̎擾
				System.Text.StringBuilder cmdQuery_y = new System.Text.StringBuilder(256);
				cmdQuery_y.Append("SELECT ");
				cmdQuery_y.Append(" SM03_1.�L���b�c ");
				cmdQuery_y.Append(",SM03_1.�L�� ");
				cmdQuery_y.Append(",NVL(SM03_2.�L���b�c, ' ') ");
				cmdQuery_y.Append(",NVL(SM03_2.�L��, ' ') ");
				cmdQuery_y.Append(" FROM \"�r�l�O�R�L��\" SM03_1 ");
				cmdQuery_y.Append(" LEFT JOIN \"�r�l�O�R�L��\" SM03_2 ");
				cmdQuery_y.Append(       " ON SM03_1.����b�c = SM03_2.����b�c ");
				cmdQuery_y.Append(      " AND SM03_1.�L���b�c = SM03_2.����b�c ");
				cmdQuery_y.Append(      " AND '0'             = SM03_2.�폜�e�f ");
				cmdQuery_y.Append("WHERE SM03_1.����b�c   = 'Jyusoshohin' "); // ���q�^���Ή�
				cmdQuery_y.Append(  "AND SM03_1.����b�c   = '0000' ");
				cmdQuery_y.Append(  "AND SM03_1.�폜�e�f   = '0' ");
				cmdQuery_y.Append("ORDER BY SM03_1.�L���b�c,SM03_2.�L���b�c \n");
				OracleDataReader reader_y = CmdSelect(sUser, conn2, cmdQuery_y);

				//�i���L���̎擾
				System.Text.StringBuilder cmdQuery_h = new System.Text.StringBuilder(256);
				cmdQuery_h.Append("SELECT ");
				cmdQuery_h.Append(" �L���b�c ");
				cmdQuery_h.Append(",�L�� ");
				cmdQuery_h.Append(" FROM \"�r�l�O�R�L��\" ");
				cmdQuery_h.Append("WHERE ����b�c   = '" + sKey[0] + "' ");
				cmdQuery_h.Append(  "AND ����b�c   = '" + sKey[1] + "' ");
				cmdQuery_h.Append(  "AND �폜�e�f   = '0' ");
				cmdQuery_h.Append("ORDER BY �L���b�c \n");
				OracleDataReader reader_h = CmdSelect(sUser, conn2, cmdQuery_h);

				bool b�A���w�� = true;
				bool b�i���L�� = true;
				string s�e�L�� = "";
				while (true)
				{
					if (b�A���w��) b�A���w�� = reader_y.Read();
					if (b�i���L��) b�i���L�� = reader_h.Read();

					string[] sData = new string[4];
					if (b�A���w��)
					{
						sData[0]  = reader_y.GetString(0).TrimEnd();
						sData[1]  = reader_y.GetString(1).TrimEnd();
					}
					else
					{
						sData[0] = "";
						sData[1] = "";
					}
					if (b�A���w�� && !sData[0].Equals(s�e�L��))
					{
						if (b�i���L��)
						{
							sData[2]  = reader_h.GetString(0).TrimEnd();
							sData[3]  = reader_h.GetString(1).TrimEnd();
						}
						else
						{
							sData[2] = "";
							sData[3] = "";
						}
						s�e�L�� = sData[0];
						alRet.Add(sData);
						if (!reader_y.GetString(2).TrimEnd().Equals(""))
						{
							sData = new string[4];
							if (b�i���L��) b�i���L�� = reader_h.Read();
							sData[0]  = "  " + reader_y.GetString(2).TrimEnd();
							sData[1]  = "�@�@�@" + reader_y.GetString(3).TrimEnd();
						}
						else
						{
							continue;
						}
					}
					else
					{
						if (b�A���w��)
						{
							sData[0]  = "  " + reader_y.GetString(2).TrimEnd();
							sData[1]  = "�@�@�@" + reader_y.GetString(3).TrimEnd();
						}
					}

					if (b�i���L��)
					{
						sData[2]  = reader_h.GetString(0).TrimEnd();
						sData[3]  = reader_h.GetString(1).TrimEnd();
					}
					else
					{
						sData[2] = "";
						sData[3] = "";
					}
					if (!b�A���w�� && !b�i���L��) break;
					alRet.Add(sData);
				}
				disposeReader(reader_y);
				disposeReader(reader_h);
				reader_y = null;
				reader_h = null;
				if (alRet.Count == 0)
				{
					sRet[0] = "�Y���f�[�^������܂���";
					alRet.Add(sRet);
				}
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
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
		// MOD 2011.06.06 ���s�j���� ���q�^���A�����i�R�[�h�����ǉ� END
		// ADD 2015.05.01 BEVAS) �O�c CM14J�X�֔ԍ����݃`�F�b�N START

		/*********************************************************************
		 * �Z���̎擾 ���q�Ή���
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�X�֔ԍ��A�Z���A�Z���b�c
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2address\Service1.asmx.cs(535):
		*/ 
		// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H START
		private static string GET_BYPOSTCODE2J_SELECT
			= "SELECT �X�֔ԍ�, �s���{����, �s�撬����, ���於, \n"
			+ " �s���{���b�c, �s�撬���b�c, �厚�ʏ̂b�c \n"
			+ " FROM �b�l�P�S�X�֔ԍ��i \n";
		// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H END
		[WebMethod]
		public String[] Get_byPostcode2(string[] sUser, string s�X�֔ԍ�)
		{
			// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
			//			logFileOpen(sUser);
			logWriter(sUser, INF, "�Z���擾�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ADD-S 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
				//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
			//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
			//			// ����`�F�b�N
			//			sRet[0] = userCheck2(conn2, sUser);
			//			if(sRet[0].Length > 0)
			//			{
			//				disconnect2(sUser, conn2);
			//				logFileClose();
			//				return sRet;
			//			}
			//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				cmdQuery
					// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
					//					= "SELECT �X�֔ԍ�, TRIM(�s���{����), TRIM(�s�撬����), TRIM(���於), \n"
					//					+        "�s���{���b�c || �s�撬���b�c || �厚�ʏ̂b�c \n"
					//					+   " FROM �b�l�P�S�X�֔ԍ��i \n";
					= GET_BYPOSTCODE2J_SELECT;
				// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
				if(s�X�֔ԍ�.Length == 7)
				{
					cmdQuery += " WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' \n";
				}
				else
				{
					cmdQuery += " WHERE �X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' \n";
				}
				cmdQuery +=    " AND �폜�e�f = '0' \n";

				// MOD-S 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
				//OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				logWriter(sUser, INF_SQL, "###�o�C���h��i�z��j###\n" + cmdQuery);	//�C���O��UPDATE�������O�o��

				cmdQuery = GET_BYPOSTCODE2J_SELECT;
				if(s�X�֔ԍ�.Length == 7)
				{
					cmdQuery += " WHERE �X�֔ԍ� = :p_YuubinNo \n";
				}
				else
				{
					cmdQuery += " WHERE �X�֔ԍ� LIKE :p_YuubinNo \n";
				}
				cmdQuery +=    " AND �폜�e�f = '0' \n";

				wk_opOraParam = new OracleParameter[1];
				if(s�X�֔ԍ�.Length == 7)
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s�X�֔ԍ�, ParameterDirection.Input);
				}
				else
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s�X�֔ԍ�+"%", ParameterDirection.Input);
				}

				OracleDataReader	reader = CmdSelect(sUser, conn2, cmdQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j

				if (reader.Read())
				{
					// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
					//					sRet[1] = reader.GetString(0);	// �X�֔ԍ�
					//					sRet[2] = reader.GetString(1)	// �s���{����
					//							+ reader.GetString(2)	// �s�撬����
					//							+ reader.GetString(3);	// ���於
					//					sRet[3] = reader.GetString(4);	// �Z���b�c
					sRet[1] = reader.GetString(0).Trim();	// �X�֔ԍ�
					sRet[2] = reader.GetString(1).Trim()	// �s���{����
						+ reader.GetString(2).Trim()	// �s�撬����
						+ reader.GetString(3).Trim();	// ���於
					sRet[3] = reader.GetString(4).Trim()	// �s���{���b�c
						+ reader.GetString(5).Trim()	// �s�撬���b�c
						+ reader.GetString(6).Trim();	// �厚�ʏ̂b�c
					// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
					sRet[0] = "����I��";
				}
				else
				{
					sRet[0] = "�Y���f�[�^������܂���";
				}
				// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
				// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
				// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
				// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
				//				logFileClose();

			}
			return sRet;
		}

		/*********************************************************************
		 * �A�b�v���[�h�f�[�^�ǉ��Q ���q�Ή�
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2otodoke\Service1.asmx.cs(1209):
		*/ 
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM �b�l�P�S�X�֔ԍ��i \n"
			;

		[WebMethod]
		public String[] otodoke_Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "���͐�A�b�v���[�h�f�[�^�ǉ��Q�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try{
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow+1] = "";

					string[] sData = sList[iRow].Split(',');
					string s�Z���b�c = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						s�Z���b�c = sData[21];
					}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
					string s����b�c = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s����b�c = sData[19];
//					}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END

//					sData[15] = sData[15].TrimEnd();
//					if(sData[15].Length == 0){
//						sRet[iRow+1] = "�X��";//���ݒ�
//						continue;
//					}
//					if(sData[15].Length != 7){
//						sRet[iRow+1] = "�X��";//�����Ɍ�肪����ꍇ
//						continue;
//					}

					//�X�֔ԍ��}�X�^�̑��݃`�F�b�N
					OracleDataReader reader;
					string cmdQuery = "";
					cmdQuery = INS_UPLOADDATA2_SELECT1
							+ "WHERE �X�֔ԍ� = '" + sData[15] + "' \n"
//�ۗ� MOD 2010.04.13 ���s�j���� �X�֔ԍ����폜���ꂽ���̏�Q�Ή� START
							+ "AND �폜�e�f = '0' \n"
//�ۗ� MOD 2010.04.13 ���s�j���� �X�֔ԍ����폜���ꂽ���̏�Q�Ή� END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow+1] = sData[15];//�Y���f�[�^����
						reader.Close();
						disposeReader(reader);
						reader = null;
						continue;
					}
					reader.Close();

					cmdQuery
						= "SELECT �폜�e�f \n"
						+   "FROM �r�l�O�Q�׎�l \n"
						+  "WHERE ����b�c = '" + sData[0] + "' \n"
						+    "AND ����b�c = '" + sData[1] + "' \n"
						+    "AND �׎�l�b�c = '" + sData[2] + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s�폜�e�f = "";
					while (reader.Read()){
						s�폜�e�f = reader.GetString(0);
						iCnt++;
					}
					reader.Close();

					if(iCnt == 1){
						//�ǉ�
						cmdQuery 
							= "INSERT INTO �r�l�O�Q�׎�l \n"
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
							+           "'" + s�Z���b�c + "', "
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//							+           "' ', \n" //����b�c
							+           "'" + s����b�c + "', \n"
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							+           "' ', \n"
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'���͎捞', \n"
							+           "'" + sData[20] + "')"
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//�㏑���X�V
						cmdQuery
							= "UPDATE �r�l�O�Q�׎�l \n"
							+    "SET �d�b�ԍ��P = '" + sData[3] + "' "
							+       ",�d�b�ԍ��Q = '" + sData[4] + "' \n"
							+       ",�d�b�ԍ��R = '" + sData[5] + "' "
							+       ",�e�`�w�ԍ��P = '" + sData[6] + "' \n"
							+       ",�e�`�w�ԍ��Q = '" + sData[7] + "' "
							+       ",�e�`�w�ԍ��R = '" + sData[8] + "' \n"
							+       ",�Z���P = '" + sData[9] + "' "
							+       ",�Z���Q = '" + sData[10] + "' \n"
							+       ",�Z���R = '" + sData[11] + "' "
							+       ",���O�P = '" + sData[12] + "' \n"
							+       ",���O�Q = '" + sData[13] + "' "
							+       ",���O�R = '" + sData[14] + "' \n"
							+       ",�X�֔ԍ� = '" + sData[15] + "' "
							+       ",�Z���b�c = '" + s�Z���b�c + "' \n"
							+       ",�J�i���� = '" + sData[16] + "' "
							+       ",��ďo�׋敪 = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//							+       ",����b�c = ' ' \n" //����b�c
							+       ",����b�c = '" + s����b�c + "' \n"
// ADD 2008.06.13 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
							+       ",����v = '" + sData[18] + "' \n"
							+       ",���[���A�h���X = ' ' "
							+       ",�폜�e�f = '0' \n"
							+       ",�o�^�o�f = ' ' \n"
							;
						if(s�폜�e�f == "1"){
							cmdQuery
								+=  ",�o�^���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",�o�^�� = '" + sData[20] + "' \n"
								;
						}
						cmdQuery
							+=      ",�X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",�X�V�o�f = '���͎捞' "
							+       ",�X�V�� = '" + sData[20] + "' \n"
							+ "WHERE ����b�c = '" + sData[0] + "' \n"
							+   "AND ����b�c = '" + sData[1] + "' \n"
							+   "AND �׎�l�b�c = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "����I��");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}

// MOD 2010.09.08 ���s�j���� �b�r�u�捞�@�\�̒ǉ� START
		/*********************************************************************
		 * �A�b�v���[�h�f�[�^�ǉ��Q  ���q�@���˗���o�^
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		/* �G�ۂŉ��̍s�ɃJ�[�\���������Ă���[F10]�L�[�������ƌ��\�[�X���Q�Ƃł��܂�
		..\is2goirai\Service1.asmx.cs(1698):
		*/
 		private static string goirai_INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM �b�l�P�S�X�֔ԍ��i \n"
			;

		private static string goirai_INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
//			= "SELECT �X�֔ԍ� \n"
//			+ " FROM �b�l�O�Q���� \n"
			= "SELECT CM02.�X�֔ԍ� \n"
			+ ", NVL(CM01.�ۗ�����e�f,'0') \n"
			+ " FROM �b�l�O�Q���� CM02 \n"
			+ " LEFT JOIN �b�l�O�P��� CM01 \n"
			+ " ON CM02.����b�c = CM01.����b�c \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
			;

		private static string goirai_INS_UPLOADDATA2_SELECT3
			= "SELECT 1 \n"
			+ " FROM �r�l�O�S������ \n"
			;

		[WebMethod]
		public String[] goirai_Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "���˗���A�b�v���[�h�f�[�^�ǉ��Q�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[(sList.Length*2) + 1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();
			OracleDataReader reader;
			string cmdQuery = "";

			sRet[0] = "";
			try{
				string s���X�֔ԍ� = "";
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				string s�d�ʓ��͐��� = "0";
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow*2+1] = "";
					sRet[iRow*2+2] = "";

					string[] sData = sList[iRow].Split(',');
					if(sData.Length != 21){
						throw new Exception("�p�����[�^���G���[["+sData.Length+"]");
					}

					string s����b�c   = sData[0];
					string s����b�c   = sData[1];
					string s�ב��l�b�c = sData[2];
					string s�X�֔ԍ�   = sData[12];
					string s������b�c = sData[17];
					string s�����敔�� = sData[18];

					if(iRow == 0){
						//����}�X�^�̑��݃`�F�b�N
						cmdQuery = goirai_INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
//								+ "WHERE ����b�c = '" + s����b�c + "' \n"
//								+ "AND ����b�c = '" + s����b�c + "' \n"
//								+ "AND �폜�e�f = '0' \n"
								+ "WHERE CM02.����b�c = '" + s����b�c + "' \n"
								+ "AND CM02.����b�c = '" + s����b�c + "' \n"
								+ "AND CM02.�폜�e�f = '0' \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
								;

						reader = CmdSelect(sUser, conn2, cmdQuery);
						if(!reader.Read()){
							reader.Close();
							disposeReader(reader);
							reader = null;
							throw new Exception("�Z�N�V���������݂��܂���");
						}
						s���X�֔ԍ� = reader.GetString(0).TrimEnd();
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
						s�d�ʓ��͐��� = reader.GetString(1).TrimEnd();
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						reader.Close();
						disposeReader(reader);
						reader = null;
					}

					//�X�֔ԍ��}�X�^�̑��݃`�F�b�N
					cmdQuery = goirai_INS_UPLOADDATA2_SELECT1
// MOD 2010.09.29 ���s�j���� �X�֔ԍ�(__)�Ή��i�������o�O���������j START
//							+ "WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' \n"
//							+ "AND �폜�e�f = '0' \n"
							;
							string s�X�֔ԍ��P = "";
							string s�X�֔ԍ��Q = "";
							if(s�X�֔ԍ�.Length > 3){
								s�X�֔ԍ��P = s�X�֔ԍ�.Substring(0,3).Trim();
								s�X�֔ԍ��Q = s�X�֔ԍ�.Substring(3).Trim();
								s�X�֔ԍ� = s�X�֔ԍ��P + s�X�֔ԍ��Q;
							}
							if(s�X�֔ԍ�.Length == 7){
								cmdQuery += " WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' \n";
							}else{
								cmdQuery += " WHERE �X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' \n";
							}
							cmdQuery += "AND �폜�e�f = '0' \n"
// MOD 2010.09.29 ���s�j���� �X�֔ԍ�(__)�Ή��i�������o�O���������j END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+1] = s�X�֔ԍ�.TrimEnd();//�Y���f�[�^����
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					//������}�X�^�̑��݃`�F�b�N
					cmdQuery = goirai_INS_UPLOADDATA2_SELECT3
							+ "WHERE �X�֔ԍ� = '" + s���X�֔ԍ� + "' \n"
							+ "AND ���Ӑ�b�c = '" + s������b�c + "' \n"
							+ "AND ���Ӑ敔�ۂb�c = '" + s�����敔�� + "' \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
							+ "AND ����b�c = '" + s����b�c + "' \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
 							+ "AND �폜�e�f = '0' \n"
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+2] = s������b�c.TrimEnd(); //�Y���f�[�^����
						if(s�����敔��.TrimEnd().Length > 0){
							sRet[iRow*2+2] += "-" + s�����敔��.TrimEnd();
						}
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;
					
					//�G���[������΁A���̍s
					if(sRet[iRow*2+1].Length != 0 || sRet[iRow*2+2].Length != 0){
						continue;
					}

					cmdQuery
						= "SELECT �폜�e�f \n"
						+   "FROM �r�l�O�P�ב��l \n"
						+  "WHERE ����b�c = '" + s����b�c + "' \n"
						+    "AND ����b�c = '" + s����b�c + "' \n"
						+    "AND �ב��l�b�c = '" + s�ב��l�b�c + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s�폜�e�f = "";
					while (reader.Read()){
						s�폜�e�f = reader.GetString(0);
						iCnt++;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					if(iCnt == 1){
						//�ǉ�
						cmdQuery 
							= "INSERT INTO �r�l�O�P�ב��l \n"
							+ "VALUES ( \n"
							+  "'" + sData[0] + "', "		//����b�c
							+  "'" + sData[1] + "', \n"		//����b�c
							+  "'" + sData[2] + "', \n"		//�ב��l�b�c

							+  "'" + sData[17] + "', "		//���Ӑ�b�c
							+  "'" + sData[18] + "', \n"	//���Ӑ敔�ۂb�c
							+  "'" + sData[3] + "', "		//�d�b�ԍ�
							+  "'" + sData[4] + "', "
							+  "'" + sData[5] + "', \n"
							+  "' ', "						//�e�`�w�ԍ�
							+  "' ', "
							+  "' ', \n"
							+  "'" + sData[6] + "', "		//�Z��
							+  "'" + sData[7] + "', "
							+  "'" + sData[8] + "', \n"
							+  "'" + sData[9] + "', "		//���O
							+  "'" + sData[10] + "', "
							+  "'" + sData[11] + "', \n"
							+  "'" + sData[12] + "', "		//�X�֔ԍ�
							+  "'" + sData[13] + "', \n"	//�J�i����
							+  " " + sData[14] + " , "		//�ː�
							+  " " + sData[15] + " , \n"	//�d��
							+  "' ', "						//�׎D�敪
							+  "'" + sData[16] + "', \n"	//���[���A�h���X
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
						//�㏑���X�V
						cmdQuery
							= "UPDATE �r�l�O�P�ב��l \n"
							+    "SET ���Ӑ�b�c = '" + sData[17] + "' \n"
							+       ",���Ӑ敔�ۂb�c = '" + sData[18] + "' \n"
							+       ",�d�b�ԍ��P = '" + sData[3] + "' \n"
							+       ",�d�b�ԍ��Q = '" + sData[4] + "' \n"
							+       ",�d�b�ԍ��R = '" + sData[5] + "' \n"
							+       ",�e�`�w�ԍ��P = ' ' \n"
							+       ",�e�`�w�ԍ��Q = ' ' \n"
							+       ",�e�`�w�ԍ��R = ' ' \n"
							+       ",�Z���P = '" + sData[6] + "' \n"
							+       ",�Z���Q = '" + sData[7] + "' \n"
							+       ",�Z���R = '" + sData[8] + "' \n"
							+       ",���O�P = '" + sData[9] + "' \n"
							+       ",���O�Q = '" + sData[10] + "' \n"
							+       ",���O�R = '" + sData[11] + "' \n"
							+       ",�X�֔ԍ� = '" + sData[12] + "' "
							+       ",�J�i���� = '" + sData[13] + "' "
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
							;
						if(s�d�ʓ��͐��� == "1"){
							cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
							+       ",�ː� = "+ sData[14] +" "
							+       ",�d�� = "+ sData[15] +" "
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
							;
						}
						cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
							+       ",�׎D�敪 = ' ' "
							+       ",\"���[���A�h���X\" = '"+ sData[16] +"' "
							+       ",�폜�e�f = '0' \n"
							;
						if(s�폜�e�f == "1"){
							cmdQuery
								+=  ",�o�^���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",�o�^�o�f = '" + sData[19] + "' "
								+   ",�o�^�� = '" + sData[20] + "' \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
								;
							if(s�d�ʓ��͐��� != "1"){
								cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
								+   ",�ː� = "+ sData[14] +" "
								+   ",�d�� = "+ sData[15] +" \n"
								;
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
							}
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						}
						cmdQuery
							+=      ",�X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",�X�V�o�f = '" + sData[19] + "' "
							+       ",�X�V�� = '" + sData[20] + "' \n"
							+ "WHERE ����b�c = '" + sData[0] + "' \n"
							+   "AND ����b�c = '" + sData[1] + "' \n"
							+   "AND �ב��l�b�c = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "����I��");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 ���s�j���� �b�r�u�捞�@�\�̒ǉ� END

// ADD 2015.05.01 BEVAS) �O�c CM14J�X�֔ԍ����݃`�F�b�N END
	}

}
