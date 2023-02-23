/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#include "ImageReaderSettingsWrapper.h"

ImageReaderSettingsWrapper::ImageReaderSettingsWrapper()
    : m_imageReaderSettingsPtr(new ImageReadSettings())
{
}

ImageReaderSettingsWrapper::~ImageReaderSettingsWrapper()
{
    this->!ImageReaderSettingsWrapper();
}

ImageReaderSettingsWrapper::!ImageReaderSettingsWrapper()
{
    if (m_imageReaderSettingsPtr)
    {
        delete m_imageReaderSettingsPtr;
        m_imageReaderSettingsPtr = nullptr;
    }
}