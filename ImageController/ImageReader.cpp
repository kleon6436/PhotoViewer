#include "pch.h"

/*!
 * @file	ImageReader.cpp
 * @author	kleon6436
 *
 */

#include "RawImageController.h"
#include "NormalImageController.h"
#include "ImageReader.h"
#include <locale.h>
#include <iostream>
#include <windows.h>

namespace Kchary::ImageController::Library
{
	using namespace Kchary::ImageController::RawImageControl;
	using namespace Kchary::ImageController::NormalImageControl;

	ImageReader::ImageReader()
	{
		m_rawImageController = std::make_unique<RawImageController>();
		m_normalImageController = std::make_unique<NormalImageController>();
	}

	bool ImageReader::LoadImageAndGetImageSize(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		m_imageReadSettings = imageReadSettings;

		bool result;
		if (imageReadSettings.isRawImage)
		{
			result = m_rawImageController->LoadImageAndGetImageSize(imagePath, imageReadSettings, imageSize);
		}
		else
		{
			result = m_normalImageController->LoadImageAndGetImageSize(imagePath, imageReadSettings, imageSize);
		}

		return result;
	}

	bool ImageReader::GetImageData(ImageData& imageData)
	{
		bool result;
		if (m_imageReadSettings.isRawImage)
		{
			result = m_rawImageController->GetImageData(imageData);
		}
		else
		{
			result = m_normalImageController->GetImageData(imageData);
		}

		return result;
	}

	bool ImageReader::GetThumbnailImageData(ImageData& imageData)
	{
		if (!m_imageReadSettings.isThumbnailMode)
		{
			// サムネイルモードで画像を読み込んでいないのでエラーとする
			return false;
		}

		if (m_imageReadSettings.isRawImage)
		{
			m_rawImageController->GetThumbnailImageData(imageData);
		}
		else
		{
			m_normalImageController->GetThumbnailImageData(imageData);
		}

		return true;
	}
}