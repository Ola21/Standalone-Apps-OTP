Imports System.Data.SqlServerCe
Imports System.Net.Mail
Imports System.Configuration
Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Web

Public Class frmLoginOTP
    Dim start As Integer = 0
    Dim expire As Integer = 600
    Dim final As Integer
    Dim rand As String = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"

    Dim con As SqlCeConnection = New SqlCeConnection(ConfigurationManager.ConnectionStrings("frzkConnect").ConnectionString)
    Dim cmd As SqlCeCommand
    Dim dr As SqlCeDataReader
    Dim exist As Boolean
    Dim txtMail
    Dim password
    Dim mobileNo
    Dim genT As DateTime
    Dim ExpT As DateTime

    Private Sub frmLoginOTP_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        txtPassword.Enabled = False
        TimerOTP.Enabled = True
        Me.AcceptButton = btnSubmit



    End Sub

    Private Sub lnkReg_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnkReg.LinkClicked
        frmRegister.Show()
        Me.Hide()

    End Sub

    Private Sub linkforgetpwd_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkforgetpwd.LinkClicked
        Dim f2 As New frmForgotPassword
        f2.Show()

        'frmForgotPassword.Show()
        Me.Hide()

    End Sub

    Private Sub lblOTP_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lblOTP.LinkClicked
        frmFrzkLogin.Show()
        Me.Hide()

    End Sub

    Private Sub lblClose_Click(sender As Object, e As EventArgs) Handles lblClose.Click
        Application.Exit()

    End Sub

    Private Sub lblMinimize_Click(sender As Object, e As EventArgs) Handles lblMinimize.Click
        Me.WindowState = FormWindowState.Minimized

    End Sub

    Private Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
        txtMail = txtEmail.Text
        Try
            con.Open()



            If rdoOTPNo.Checked = True Then

                OMfinalCheck()

            End If
            If rdoOTPYes.Checked = True Then

                expiry()

            End If

        Catch ex As Exception

            MessageBox.Show(ex.ToString)
        Finally
            con.Close()

        End Try



    End Sub
    'get Mobile number to send OTP
    Public Function check()

        Try
            Dim mobil As String = "select * from tblReg where Email=@Email"

            cmd = New SqlCeCommand(mobil, con)

            cmd.Parameters.AddWithValue("@Email", txtEmail.Text)

            dr = cmd.ExecuteReader

            While dr.Read
                mobileNo = dr.GetString(3)
            End While

        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
        Return mobileNo
    End Function
    Dim hasrow As Integer = 0
    Public Function mails()
        Try
            Dim check As String = "select * from tblReg where Email=@Email"

            cmd = New SqlCeCommand(check, con)

            cmd.Parameters.AddWithValue("@Email", txtEmail.Text)

            dr = cmd.ExecuteReader

            While dr.Read

                If txtEmail.Text = dr.GetString(2) Then

                    hasrow = 1

                Else
                    ' exist = False
                    hasrow = 2


                End If
            End While

            If hasrow = 1 Then
                ' MessageBox.Show(" Exist")
            Else
                MessageBox.Show("Invalid Email. Check if email is correct")
                txtEmail.Text = ""
                txtEmail.Focus()
            End If

            ' MessageBox.Show("" + e.ToString)
        Catch ex As Exception
            MessageBox.Show(ex.ToString)

        Finally

        End Try
        Return hasrow
    End Function
    'generate OTP using random number and alphabets
    Public Function genOTP()
        Dim OTP As New Random
        Dim count = 0
        Dim strpos = ""
        Dim res = ""

        While count <= 6
            strpos = OTP.Next(0, rand.Length)
            res += rand(strpos)

            count += 1
        End While
        ' MessageBox.Show("" + res)
        password = res
        Return password
    End Function
    'generate OTP using timestamp
    Public Function genTimeStampOTP()
        Dim pwdOTP = CLng(DateTime.UtcNow.Subtract(New DateTime(1970, 1, 1)).TotalMilliseconds)
        Dim newOTP = Convert.ToString(pwdOTP)

        Dim OTP As New Random
        Dim count = 0
        Dim strpos = ""
        Dim res = ""

        While count < 6
            strpos = OTP.Next(0, newOTP.Length)
            res += newOTP(strpos)

            count += 1
        End While

        password = res

        Return password
    End Function

    Public Sub saveSendOTP()
        genTimeStampOTP()
        Dim pwd As String = password

        ' starts countdown to another 10 minutes before the OTP expires. DB is updated after OTP expires
        TimerOTP.Start()
        check()

        Try
            Dim d As Date = DateAndTime.Now
            Dim d1 As Date = DateTime.Now.AddMinutes(10)
            saveOTP(password, d, d1)
            mailOTP(password)
            smsOTP(password, mobileNo)

        Catch ex As Exception
            MessageBox.Show(ex.ToString)

        Finally


        End Try



    End Sub

    'save OTP to DB
    Public Sub saveOTP(ByVal pass As String, ByVal gentime As String, ByVal exptime As String)
        Dim save As String = "insert into tblOTP(Email,OTP,Valid,Expire,GenTime,ExpTime)values(@Email,@OTP,@Valid,@Expire,@GenTime,@ExpTime)"

        Try
            cmd = New SqlCeCommand(save, con)
            cmd.Parameters.AddWithValue("@Email", txtEmail.Text)
            cmd.Parameters.AddWithValue("@OTP", pass)
            cmd.Parameters.AddWithValue("@Valid", 0)
            cmd.Parameters.AddWithValue("@Expire", 0)
            cmd.Parameters.AddWithValue("@GenTime", gentime)
            cmd.Parameters.AddWithValue("@ExpTime", exptime)

            cmd.ExecuteNonQuery()

        Catch ex As Exception
            MessageBox.Show("" + ex.ToString)
        End Try

    End Sub
    ' Send OTP to email addresses
    Public Sub mailOTP(ByVal pass As String)
        Dim mail As New MailMessage
        Dim SupportMail = "help.frzk@gmail.com"
        Dim Usermail = txtEmail.Text

        Dim gmailsmtp = "smtp.gmail.com"
        Dim yahoosmtp = "smtp.mail.yahoo.com"
        Dim outlooksmtp = "smtp-mail.outlook.com"

        Dim body As String = "Your One Time Password is : " & pass
        Try
            If Usermail.Contains("outlook.com") Then
                'MessageBox.Show("Outlook SMTP")
                mail.From = New MailAddress(SupportMail)
                mail.To.Add(Usermail)
                mail.Subject = "Your One time Password!"
                mail.Body = body

                Dim smtp As New SmtpClient(outlooksmtp)

                smtp.Port = 587
                smtp.EnableSsl = True
                smtp.Credentials = New System.Net.NetworkCredential(SupportMail, "Your EMail Password")

                smtp.Send(mail)

            End If

            If Usermail.Contains("yahoo.com") Then
                'MessageBox.Show("Yahoo SMTP")
                mail.From = New MailAddress(SupportMail)
                mail.To.Add(Usermail)
                mail.Subject = "Your One time Password!"
                mail.Body = body

                Dim smtp As New SmtpClient(yahoosmtp)

                smtp.Port = 25
                smtp.EnableSsl = True
                smtp.Credentials = New System.Net.NetworkCredential(SupportMail, "Your EMail Password")

                smtp.Send(mail)

            End If

            If Usermail.Contains("gmail.com") Then
                '    MessageBox.Show("gmail SMTP")
                mail.From = New MailAddress(SupportMail)
                mail.To.Add(Usermail)
                mail.Subject = "Your One time Password!"
                mail.Body = body

                Dim smtp As New SmtpClient(gmailsmtp)

                smtp.Port = 587
                smtp.EnableSsl = True
                smtp.Credentials = New System.Net.NetworkCredential(SupportMail, "Your EMail Password")

                smtp.Send(mail)

            End If




            MessageBox.Show("OTP sent!. Check your email for password!")




        Catch ex As Exception
            MessageBox.Show(ex.Message)
        Finally

        End Try


    End Sub
' using Ozeki sms gateway for sms testing
    Public Sub smsOTP(ByVal pass As String, ByVal ClientNo As Integer)

        Dim request As HttpWebRequest
        Dim response As HttpWebResponse = Nothing
        Dim url As String
        Dim username As String = "admin"
        Dim password As String = "password"
        Dim host As String = "http://127.0.0.1:9501"
        Dim originator As String = "09967469401"
        Dim body As String = "Your One Time Password is : " & pass

        Try

            url = host + "/api?action=sendmessage&" _
                     & "username=" & HttpUtility.UrlEncode(username) _
                     & "&password=" + HttpUtility.UrlEncode(password) _
                     & "&recipient=" + HttpUtility.UrlEncode(ClientNo) _
                     & "&messagetype=SMS:TEXT" _
                     & "&messagedata=" + HttpUtility.UrlEncode(body) _
                     & "&originator=" + HttpUtility.UrlEncode(originator) _
                     & "&serviceprovider=" _
                     & "&responseformat=html"


            request = DirectCast(WebRequest.Create(url), HttpWebRequest)

            response = DirectCast(request.GetResponse(), HttpWebResponse)

            ' MessageBox.Show("Response: " & response.StatusDescription)
            MessageBox.Show("OTP sent!. Check your Mobile Number for password!")
        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub
    ' timer counting up to expire time =600seconds=10mins
    Private Sub TimerOTP_Tick(sender As Object, e As EventArgs) Handles TimerOTP.Tick
        start += 1


        If start = expire Then

            TimerOTP.Stop()

            Try
                con.Open()
                ' calls the method to update the Valid and Expire columns in the DB
                updtodatevalid(txtMail)

            Catch ex As Exception
                MessageBox.Show("" + ex.ToString)
            Finally
                con.Close()

            End Try
        End If

    End Sub

    'Public Sub UpdateExpire()
    '    Dim d As Date = DateAndTime.Now
    '    Dim d1 As Date = DateTime.Now.AddMinutes(30)
    '    'Dim d2 As New DateTime(2016, 6, 20, 22, 34, 0)

    '    Dim t As TimeSpan
    '    t = d1.Subtract(d)
    '    Dim ts = Convert.ToInt32(t.TotalMinutes)
    '    MessageBox.Show(d1)
    'End Sub
    'Public Function OTPExp()
    '    Dim d As Date = DateAndTime.Now
    '    Dim d1 As Date = DateTime.Now.AddMinutes(30)
    '    Return d
    '    Return d1
    'End Function

    Private Sub rdoOTPYes_CheckedChanged(sender As Object, e As EventArgs) Handles rdoOTPYes.CheckedChanged
        If rdoOTPYes.Checked = True Then
            txtEmail.Clear()

            txtPassword.Enabled = True
            txtEmail.Focus()

            btnSubmit.Text = "Submit"
        End If
    End Sub

    Private Sub rdoOTPNo_CheckedChanged(sender As Object, e As EventArgs) Handles rdoOTPNo.CheckedChanged
        If rdoOTPNo.Checked = True Then
            txtEmail.Clear()
            txtEmail.Focus()
            txtPassword.Clear()



            txtPassword.Enabled = False
            btnSubmit.Text = "Generate OTP"
        End If
    End Sub
    Public Sub login()
        Try
            Dim sel As String = "select * from tblOTP where Email=@Email and Valid=0 and Expire=0"

            cmd = New SqlCeCommand(sel, con)
            cmd.Parameters.AddWithValue("@Email", txtEmail.Text)
            dr = cmd.ExecuteReader

            While dr.Read
                If dr.GetString(1) = txtEmail.Text And dr.GetString(2) = txtPassword.Text Then
                    MessageBox.Show("Login Successful !")
                    frmMain.Show()
                    Me.Hide()

                    updtodatevalid(txtEmail.Text)

                Else
                    MessageBox.Show("Login is not Successful Check Password !")
                End If

            End While


        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub

    'update table after login set expire and valid to 1
    Public Sub updtodatevalid(ByVal email As String)

        Try
            Dim upd As String = "update tblOTP set Valid=1, Expire=1 where Email=@Email and Valid=0 and Expire=0"
            cmd = New SqlCeCommand(upd, con)
            cmd.Parameters.AddWithValue("@Email", email)
            cmd.ExecuteNonQuery()

        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try


        'MessageBox.Show("Uptodate")
    End Sub
    Private Sub expiry()
        Try
            Dim curtime = DateTime.Now

            Dim sql = "select * from tblOTP where Email=@Email and Valid=0 and Expire=0"

            cmd = New SqlCeCommand(sql, con)

            cmd.Parameters.AddWithValue("@Email", txtEmail.Text)

            dr = cmd.ExecuteReader

            While dr.Read()
                genT = dr.GetDateTime(5)
                ExpT = dr.GetDateTime(6)
            End While



            If curtime < genT Then

                MessageBox.Show("OTP not valid. Check your System Date and Time")
            ElseIf curtime > genT And curtime > ExpT Then
                MessageBox.Show("OTP is Expired")
                    updtodatevalid(txtEmail.Text)




                Else

                    'OTP is valid simply Login
                    login()


                End If

            ' End If



        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub

    Private Sub OMfinalCheck()

        Try

            Dim curtime = DateTime.Now

            Dim sql = "select * from tblOTP where Email=@Email and Valid=0 and Expire=0"

            cmd = New SqlCeCommand(sql, con)

            cmd.Parameters.AddWithValue("@Email", txtEmail.Text)

            dr = cmd.ExecuteReader

            While dr.Read
                genT = dr.GetDateTime(5)
                ExpT = dr.GetDateTime(6)
            End While

            If curtime > genT And curtime < ExpT Then

                Dim result As Integer = MessageBox.Show("Do you want to Cancel OTP ", "There is Valid OTP  for user", MessageBoxButtons.YesNoCancel)
                If result = DialogResult.Yes Then
                    MessageBox.Show("OTP is canceled. Generate a New OTP")
                    updtodatevalid(txtEmail.Text)
                    txtEmail.Clear()
                    txtEmail.Focus()




                ElseIf result = DialogResult.No Then
                    MessageBox.Show("OTP is valid. Login Using The OTP")
                    rdoOTPYes.Checked = True

                ElseIf result = DialogResult.Cancel Then

                    frmFrzkLogin.Show()
                    Me.Hide()

                End If
            Else

                'checks if the email is a valid email in the list of registered Users.
                mails()

                'hasrow is used to determine if there current data in the DB and checks in Db for Email and to regen password for such email.

                Dim valid = hasrow

                If valid = 1 Then


                    saveSendOTP()
                    txtEmail.Clear()


                End If


            End If
            '  MessageBox.Show("Invalid OTP")


        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try

    End Sub

End Class
