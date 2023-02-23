/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#include "ImageDataWrapper.h"

ImageDataWrapper::ImageDataWrapper()
    : m_imageDataPtr(new ImageData())
{
}

ImageDataWrapper::~ImageDataWrapper()
{
    this->!ImageDataWrapper();
}

ImageDataWrapper::!ImageDataWrapper()
{
    if (m_imageDataPtr)
    {
        delete m_imageDataPtr;
        m_imageDataPtr = nullptr;
    }
}