/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#include "ImageReaderWrapper.h"
#include < vcclr.h >

using namespace Kchary::ImageController::Library;

ImageReaderWrapper::ImageReaderWrapper()
	: m_imageReaderPtr(new ImageReader())
{
}

ImageReaderWrapper::~ImageReaderWrapper()
{
	this->!ImageReaderWrapper();
}

ImageReaderWrapper::!ImageReaderWrapper()
{
	if (m_imageReaderPtr)
	{
		delete m_imageReaderPtr;
		m_imageReaderPtr = nullptr;
	}
}

System::Boolean ImageReaderWrapper::GetImageData(System::String^ imagePath, ImageReaderSettingsWrapper^ imageReaderSettings, ImageDataWrapper^ imageData)
{
	pin_ptr<const wchar_t> path = PtrToStringChars(imagePath);
	try
	{
		m_imageReaderPtr->GetImageData(path, *imageReaderSettings->m_imageReaderSettingsPtr, *imageData->m_imageDataPtr);
	}
	catch (...)
	{
		return false;
	}

	return true;
}
