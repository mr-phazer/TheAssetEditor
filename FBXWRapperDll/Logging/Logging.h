#pragma once

#include <Windows.h>
//#include "..\..\QtRME_GUI\constants.h"
//#include <qdesktopwidget.h>
//#include <qapplication.h>
#include <fstream>
#include <string>


#include <iostream>
#include <locale>
#include <codecvt>

static void WriteToLogFile(const std::string& logString)
{
	std::ofstream oOutFile(L"fbxsdk.log.txt", std::ios::app);
	oOutFile << logString;
	oOutFile.close();
}

static std::wstring WidenStr(const std::string& str)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;

	return converterX.from_bytes(str);
}

static std::string NarrowStr(const std::wstring& wstr)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;

	return converterX.to_bytes(wstr);
}



namespace ConsoleForeground
{
	enum {
		BLACK = 0,
		DARKBLUE = FOREGROUND_BLUE,
		DARKGREEN = FOREGROUND_GREEN,
		DARKCYAN = FOREGROUND_GREEN | FOREGROUND_BLUE,
		DARKRED = FOREGROUND_RED,
		DARKMAGENTA = FOREGROUND_RED | FOREGROUND_BLUE,
		DARKYELLOW = FOREGROUND_RED | FOREGROUND_GREEN,
		DARKGRAY = FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE,
		GRAY = FOREGROUND_INTENSITY,
		BLUE = FOREGROUND_INTENSITY | FOREGROUND_BLUE,
		GREEN = FOREGROUND_INTENSITY | FOREGROUND_GREEN,
		CYAN = FOREGROUND_INTENSITY | FOREGROUND_GREEN | FOREGROUND_BLUE,
		RED = FOREGROUND_INTENSITY | FOREGROUND_RED,
		MAGENTA = FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_BLUE,
		YELLOW = FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN,
		WHITE = FOREGROUND_INTENSITY | FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE,
	};
}

namespace ConsoleBackground
{
	enum {
		BLACK = 0,
		DARKBLUE = BACKGROUND_BLUE,
		DARKGREEN = BACKGROUND_GREEN,
		DARKCYAN = BACKGROUND_GREEN | BACKGROUND_BLUE,
		DARKRED = BACKGROUND_RED,
		DARKMAGENTA = BACKGROUND_RED | BACKGROUND_BLUE,
		DARKYELLOW = BACKGROUND_RED | BACKGROUND_GREEN,
		DARKGRAY = BACKGROUND_RED | BACKGROUND_GREEN | BACKGROUND_BLUE,
		GRAY = BACKGROUND_INTENSITY,
		BLUE = BACKGROUND_INTENSITY | BACKGROUND_BLUE,
		GREEN = BACKGROUND_INTENSITY | BACKGROUND_GREEN,
		CYAN = BACKGROUND_INTENSITY | BACKGROUND_GREEN | BACKGROUND_BLUE,
		RED = BACKGROUND_INTENSITY | BACKGROUND_RED,
		MAGENTA = BACKGROUND_INTENSITY | BACKGROUND_RED | BACKGROUND_BLUE,
		YELLOW = BACKGROUND_INTENSITY | BACKGROUND_RED | BACKGROUND_GREEN,
		WHITE = BACKGROUND_INTENSITY | BACKGROUND_RED | BACKGROUND_GREEN | BACKGROUND_BLUE,
	};
}


//#define CONSOLE_OUT

//#include <F:\Qt\5.15.2\msvc2019_64\include/QtWidgets/qplaintextedit.h>
//#include <F:\Qt\5.15.2\msvc2019_64\include/QtWidgets/qwidget.h>
//
//#include <F:\Qt\5.15.2\msvc2019_64\include/QtGui/qpainter.h>
//
//#include <F:\Qt\5.15.2\msvc2019_64\include/QtCore/qdebug.h>
//#include <F:\Qt\5.15.2\msvc2019_64\include/QtCore/qtextstream.h>
//

//#include <qplaintextedit.h>
//#include <qwidget.h>

//#define CONSOLE




#ifdef CONSOLE
// Target Windows 7 SP1
#include <WinSDKVer.h>
#define _WIN32_WINNT 0x0601
#include <SDKDDKVer.h>


#include <iostream>

#include <boost/log/common.hpp>
#include <boost/log/expressions.hpp>

#include <boost/log/utility/setup/file.hpp>
#include <boost/log/utility/setup/console.hpp>
#include <boost/log/utility/setup/common_attributes.hpp>

#include <boost/log/attributes/timer.hpp>
#include <boost/log/attributes/named_scope.hpp>

#include <boost/log/sources/logger.hpp>

#include <boost/log/support/date_time.hpp>

#pragma region log_stuff
namespace logging = boost::log;
namespace sinks = boost::log::sinks;
namespace attrs = boost::log::attributes;
namespace src = boost::log::sources;
namespace expr = boost::log::expressions;
namespace keywords = boost::log::keywords;


enum severity_level
{
	normal,
	notification,
	warning,
	error,
	critical
};

// The formatting logic for the severity level
template< typename CharT, typename TraitsT >
inline std::basic_ostream< CharT, TraitsT >& operator<< (
	std::basic_ostream< CharT, TraitsT >& strm, severity_level lvl)
{
	static const char* const str[] =
	{
		"normal",
		"notification",
		"warning",
		"error",
		"critical"
	};
	if (static_cast<std::size_t>(lvl) < (sizeof(str) / sizeof(*str)))
		strm << str[lvl];
	else
		strm << static_cast<int>(lvl);
	return strm;
}
#endif
#pragma endregion 

#include <fstream>

extern void winPrint(const std::wstring& str, WORD Color = ConsoleForeground::WHITE | ConsoleBackground::BLACK);

extern void initLog();

class QConsole
{

public:
	QConsole();
	~QConsole() {
		oOutFile.close();
	}

	void write(
		const std::string& _str = "",
		const std::string& _color = "black",
		const std::string& _bg = "white",
		size_t size = 5);

	void writeLn(
		const std::string& _str = "",
		const std::string& _color = "black",
		const std::string& _bg = "white",
		size_t size = 5);

	std::ofstream oOutFile;
};

//extern void _create_console(QWidget* parent);
//extern void _log_action2(const std::string& _strMsg, bool _bSuccess);

#define ATTEMPT_BOOL(FUNCCALL) \
	{\
	_log_action("Attempting: " #FUNCCALL) \
	bool result = FUNCCALL;\
	if (result) \
	_log_action_success(#FUNCCALL) \
	else\
	_log_action_error(#FUNCCALL) \
	}\

#define ATTEMPT_HR(FUNCCALL) \
	{\
	_log_action("Attempting: " #FUNCCALL) \
	HRESULT result = FUNCCALL;\
	if (SUCCEEDED(result)) \
	_log_action_success(#FUNCCALL) \
	else\
	_log_action_error(#FUNCCALL) \
	}\


//#ifdef CONSOLE_OUT
#if 1

#define FULL_FUNC_INFO(_MSG) (std::string(__func__) +  std::string(": Line: ") + std::to_string(__LINE__) + ": " + _MSG)

#define _log_action_error(msg) _impl__log_action_error(FULL_FUNC_INFO(msg));
#define _log_action_error_with_box(parentView, msg) _impl__log_action_error_with_box(parentView, FULL_FUNC_INFO(msg));

#define _log_action(_MSG)  _impl__log_action( \
std::string(__func__) +  std::string(": Line: ") + std::to_string(__LINE__) + ": " + _MSG);\

#define _log_function_call() _impl__log_action( \
std::string(__func__) +  std::string(": Line: ") + std::to_string(__LINE__));\

#define _log_action_success(_MSG) _impl__log_action_success(_MSG);
#define _log_action_warning(_MSG) _impl__log_action_warning(_MSG);

#define _log_write(_MSG) _impl__log_write(_MSG);
//#define _log_action_success(_MSG) _impl_log_action_success(_MSG);

#else
#define _log_function_call() _true();
#define _log_action_error(msg) _false();
#define _log_action(_MSG) _true();
#define _log_action_success(_MSG) _true();
#define _log_action_warning(_MSG) _true();
#define _log_write(_MSG) _true();
#endif

extern void _impl__log_info(const std::string& _strMsg);

extern void _impl__log_action(const std::string& _strMsg);

extern void _impl__log_action_success(const std::string& _strMsg);

//void WriteToLogFile(std::stringstream& logString);

extern bool _impl__log_action_error(const std::string& _strMsg/*, QWidget* _parentWidget = nullptr*/);
//extern bool _impl__log_action_error_with_box(QWidget* parent, const std::string& _strMsg, QWidget* _parentWidget = nullptr);

extern bool _impl__log_action_warning(const std::string& _strMsg);
extern bool _impl__log_action_warning(const std::wstring& _strMsg);

extern void _impl__log_write(const std::string& _strMsg);

//extern QConsole* g_poConsole;

/*
struct CharCell
{
	enum FormatFlags : uint8_t // flags can be combine
	{
		None = 0,
		Bold = 0b01,
		Italic = 0b10,
		Underline = 0b100
	};

	char character; // ASCI character, X in an 2d array of image data
	uint8_t textColor; // Y, so that the image data need is found imagedata = [character][color];
	uint8_t backgrundColor;
	FormatFlags formatFlags;
};


/*
class CharScreenBuffer : public QWidget
{
	Q_OBJECT

public:
	CharScreenBuffer(QWidget* parent)
		: QWidget(parent)
	{
		setWindowFlag(Qt::WindowType::Window, true);
		setMinimumSize(1024, 1024);
		show();
	}

	QString lastAction;
	QString lastError;

protected:
	void writeChar(int x, int y, char character, const QColor& textColor = QColorConstants::White, const QColor& backGroundColor = QColorConstants::Black)
	{
		QPainter painter(this);
		painter.setFont(QFont("Mono", 8, QFont::PreferAntialias));
		painter.setPen(textColor);

		painter.setBackgroundMode(Qt::BGMode::OpaqueMode);
		painter.setBackground(Qt::GlobalColor::black);

		int char_width = width() / 80;
		int char_height = height() / 40;

		QRect charRect(
			x,
			y,
			x + char_width,
			y + char_height
		);

		QString tempString;
		tempString += character;

		painter.drawText(charRect, Qt::AlignLeft, tempString);
	}

	void paintEvent(QPaintEvent* event) override
	{
		for (int y = 0; y < 40; y++)
		{
			for (int x = 0; x < 80; x++)
			{
				writeChar(x, y, 'A');
			}
		}
	}
};
*/

//Console::QConsole()
//{
//	//setWindowTitle("Log Window");
//
//	oOutFile.open(L"log.txt", std::ios::app);
//
//	std::string strDate = __DATE__ + std::string(" : ");
//	strDate += __TIME__;
//
//	oOutFile << "-----------------------------------------------" << std::endl;
//	oOutFile << "'Qt RME' Log" << std::endl;
//	oOutFile << strDate << std::endl;
//	oOutFile << "-----------------------------------------------" << std::endl;
//
//	oOutFile.close();
//	/*setWindowFlag(Qt::Popup);
//	setWindowFlag(Qt::Tool);
//	setWindowFlag(Qt::PopupFocusReason);*/
//
//	//setStyleSheet("QPlainTextEdit {background-color: black; color: white;}");
//	/*setStyleSheet("QPlainTextEdit {"
//		"font: 14px console;"
//		"background-color: black;"
//		"color: white;"
//		";}"
//	);*/
//
//	//QSize size;
//
//	//QRect rec = QApplication::desktop()->screenGeometry();
//	//int height = rec.height() / 2;
//	//int width = rec.width() / 2;
//
//	//setMinimumSize(width, height);
//
//	////appendHtml("****** RMEditor 0.9.1a Log Log ******");
//	////toHtml();
//
//	//show();
//}


