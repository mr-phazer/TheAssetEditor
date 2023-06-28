#include "Logging.h"
#include <fstream>
#include <fstream>
#include <iostream>
#include <sstream>
#include <iostream>


class WinConcole
{
public:
	static void Print(const std::wstring& str, WORD Color = ConsoleBackground::BLACK | ConsoleForeground::WHITE)
	{
		HANDLE h = GetStdHandle(STD_OUTPUT_HANDLE);
		WORD wOldColorAttrs;
		CONSOLE_SCREEN_BUFFER_INFO csbiInfo;

		/*
		 * First save the current color information
		 */
		//GetConsoleScreenBufferInfo(h, &csbiInfo);
		//wOldColorAttrs = csbiInfo.wAttributes;

		/*
		 * Set the new color information
		 */
		SetConsoleTextAttribute(h, Color);

		DWORD dwChars = 0;
		WriteConsole(h, str.data(), (DWORD)str.size(), &dwChars, NULL);

		
		/*
		* Set default color info
		*/
		SetConsoleTextAttribute(h, ConsoleBackground::BLACK | ConsoleForeground::WHITE);
		
	}
};

void QConsole::writeLn(const std::string& _strMsg, const std::string& _color, const std::string& _bg, size_t size)
{
	WinConcole::Print(L"FBX SDK ACTION:");
	WinConcole::Print(L" ");
	WinConcole::Print(WidenStr(_strMsg));
	WinConcole::Print(L"\r\n");
}



void _impl__log_action(const std::string& _strMsg)
{	
	WinConcole::Print(L"FBX SDK ACTION:", ConsoleBackground::DARKBLUE | ConsoleForeground::WHITE);
	WinConcole::Print(L" ");
	WinConcole::Print(WidenStr(_strMsg));
	WinConcole::Print(L"\r\n");

	std::stringstream logString;
	logString << std::endl << "ACTION: " << (_strMsg).c_str();

	std::ofstream oOutFile(L"fbxsdk.log.txt", std::ios::app);
	//g_poConsole->oOutFile << std::endl << "ACTION: " << _strMsg;
	oOutFile << logString.str();
	oOutFile.close();
}

void _impl__log_info(const std::string& _strMsg)
{	
	WinConcole::Print(L"FBX SDK INFO:", ConsoleBackground::DARKBLUE | ConsoleForeground::WHITE);
	WinConcole::Print(L" ");
	WinConcole::Print(WidenStr(_strMsg));
	WinConcole::Print(L"\r\n");

	std::stringstream logString;
	logString << std::endl << "ACTION: " << (_strMsg).c_str();

	std::ofstream oOutFile(L"fbxsdk.log.txt", std::ios::app);
	//g_poConsole->oOutFile << std::endl << "ACTION: " << _strMsg;
	oOutFile << logString.str();
	oOutFile.close();
}

void _impl__log_action_success(const std::string& _strMsg)
{	
	WinConcole::Print(L"FBX SDK ACTION: SUCCESS:", ConsoleBackground::DARKGREEN | ConsoleForeground::WHITE);
	WinConcole::Print(L" ");
	WinConcole::Print(WidenStr(_strMsg));
	WinConcole::Print(L"Success.", ConsoleBackground::BLUE | ConsoleForeground::WHITE);
	WinConcole::Print(L"\r\n");

	std::stringstream logString;
	logString << std::endl << "FBX SDK ACTION: SUCCESS:" << _strMsg << ". Success.";
	
	WriteToLogFile(logString.str());	
}


bool _impl__log_action_error(const std::string& _strMsg/*, QWidget* _parentWidget*/)
{
	/*std::ofstream logFile("fbxsdk.log.txt", std::ios::app);;
	logFile << "Error: ";
	logFile << _strMsg.c_str();
	logFile << std::endl;*/

	//QMessageBox::critical(_parentWidget, "Error!", _strMsg.c_str());

	WinConcole::Print(L"FBX SDK ERROR:", ConsoleBackground::RED | ConsoleForeground::YELLOW);
	WinConcole::Print(L" ");
	WinConcole::Print(WidenStr(_strMsg));
	WinConcole::Print(L"\r\n");





	std::stringstream logString;
	logString << std::endl << "ERROR!: " << _strMsg;
	//std::cout << logString.str();



	std::ofstream oOutFile(L"fbxsdk.log.txt", std::ios::app);
	//g_poConsole->oOutFile << std::endl << "ACTION: " << _strMsg;
	oOutFile << logString.str();
	oOutFile.close();
	return false;

	//g_poConsole->writeLn();
	//g_poConsole->write("ERROR:", "white", "red", 4);

	//g_poConsole->write("&#160;", "white", "black", 4);
	//g_poConsole->write("&#160;", "white", "black", 4);

	//g_poConsole->write((_strMsg).c_str(), "white", "black", 4);

	//g_poConsole->write("&#160;", "white", "black", 4);
	//g_poConsole->write("&#160;", "white", "black", 4);
	////g_poConsole->write("Error!", "white", "red", 4);

	//g_poConsole->oOutFile.open(L"fbxsdk.log.txt", std::ios::app);
	////g_poConsole->oOutFile << std::endl << "ERROR!: " << _strMsg;	
	//g_poConsole->oOutFile << logString.str();

	//g_poConsole->oOutFile.close();

	//// write to system CONSOLE if any exists
	//std::cout << std::endl << "ERROR!: " << _strMsg << endl;

	//return false;
}

//bool _impl__log_action_error_with_box(QWidget* parent, const std::string& _strMsg, QWidget* _parentWidget)
//{
//	g_poConsole->writeLn();
//	g_poConsole->write("ERROR:", "white", "red", 4);
//
//	g_poConsole->write("&#160;", "white", "black", 4);
//	g_poConsole->write("&#160;", "white", "black", 4);
//
//	g_poConsole->write((_strMsg).c_str(), "white", "black", 4);
//
//	g_poConsole->write("&#160;", "white", "black", 4);
//	g_poConsole->write("&#160;", "white", "black", 4);
//	//g_poConsole->write("Error!", "white", "red", 4);
//
//	g_poConsole->oOutFile.open(L"fbxsdk.log.txt", std::ios::app);
//	g_poConsole->oOutFile << std::endl << "ERROR!: " << _strMsg;
//	g_poConsole->oOutFile.close();
//
//	// write to system CONSOLE if any exists
//	std::cout << std::endl << "ERROR!: " << _strMsg << endl;
//
//	QMessageBox::warning(parent, "Error",
//		_strMsg.c_str()
//	);
//
//	return false;
//}

bool _impl__log_action_warning(const std::string& _strMsg)
{
	std::stringstream logString;
	logString << std::endl << "Warning: " << _strMsg;
	//std::cout << logString.str();

	WinConcole::Print(L"FBX SDK WARNING:", ConsoleBackground::MAGENTA | ConsoleForeground::WHITE);
	WinConcole::Print(L" ");
	WinConcole::Print(WidenStr(_strMsg));
	WinConcole::Print(L"\r\n");



	//g_poConsole->writeLn();
	//g_poConsole->write("Warning:", "white", "purple", 4);

	//g_poConsole->write("&#160;", "white", "black", 4);
	//g_poConsole->write("&#160;", "white", "black", 4);
	//g_poConsole->write((_strMsg).c_str(), "white", "black", 4);

	//g_poConsole->write("&#160;", "white", "black", 4);
	//g_poConsole->write("&#160;", "white", "black", 4);
	//g_poConsole->write("Success.", "white", "green", 4);

	//g_poConsole->oOutFile.open(L"fbxsdk.log.txt", std::ios::app);
	////g_poConsole->oOutFile << std::endl << "Warning: " << _strMsg;
	//g_poConsole->oOutFile << logString.str();
	//g_poConsole->oOutFile.close();

	// write to system CONSOLE if any exists




	//g_poConsole->writeLn();
	//g_poConsole->write("ACTION:", "white", "blue", 4);
	//g_poConsole->write("&#160;");
	//g_poConsole->write((_strMsg + " - ").c_str(), "red", "white", 4);

	//g_poConsole->write("Warning!", "purple", "white", 4);

	return false;
}

bool _impl__log_action_warning(const std::wstring& _wstrMsg)
{
	return _impl__log_action_warning(NarrowStr(_wstrMsg));
}

void _impl__log_write(const std::string& _strMsg)
{

	std::ofstream oOutFile(L"fbxsdk.log.txt", std::ios::app);
	//g_poConsole->oOutFile << std::endl << "ACTION: " << _strMsg;
	oOutFile << _strMsg;
	oOutFile.close();
}
