/**
 * @file	NormalImageController.cpp
 * @author	kleon6436
 */

#include "NormalImageController.h"
#include <Windows.h>

namespace Kchary::ImageController::NormalImageControl
{
	bool NormalImageController::GetImageData(const wchar_t* path, const ImageReadSettings& imageReadSettings, ImageData& imageData)
	{
		// OpenCVは、wchar_t -> charに変換しないと読めないので、ここで変換処理を行う
		const auto pathStr = std::string(ConvertWcharToString(path));

		try
		{
			if (imageReadSettings.isThumbnailMode)
			{
				// 画像読み込み
				auto image = cv::imread(pathStr, GetImreadMode(imageReadSettings.resizeLongSideLength));

				const auto longSideLength = image.cols > image.rows ? image.cols : image.rows;
				const auto ratio = static_cast<double>(imageReadSettings.resizeLongSideLength) / static_cast<double>(longSideLength);
				cv::Mat resizeImage;
				cv::resize(image, resizeImage, cv::Size(), ratio, ratio, cv::INTER_AREA);

				m_image = resizeImage;
			}
			else
			{
				m_image = cv::imread(pathStr, cv::ImreadModes::IMREAD_COLOR);
			}

			const auto dataSize = m_image.total() * m_image.elemSize() * sizeof(std::byte);
			imageData.buffer.resize(dataSize);
			memcpy(&imageData.buffer[0], m_image.data, dataSize);

			imageData.size = static_cast<unsigned int>(dataSize);
			imageData.stride = static_cast<int>(m_image.step);
			imageData.width = m_image.cols;
			imageData.height = m_image.rows;
		}
		catch (...)
		{
			return false;
		}

		return true;
	}

	cv::ImreadModes NormalImageController::GetImreadMode(const int resizeLongSideLength)
	{
		cv::ImreadModes imreadMode;

		if (resizeLongSideLength <= 800)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
		}
		else if (800 < resizeLongSideLength && resizeLongSideLength <= 1600)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
		}
		else if (1600 < resizeLongSideLength && resizeLongSideLength <= 3200)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
		}
		else
		{
			imreadMode = cv::ImreadModes::IMREAD_COLOR;
		}

		return imreadMode;
	}

	std::string NormalImageController::ConvertWcharToString(const wchar_t* imagePath)
	{
		setlocale(LC_CTYPE, "ja_JP.UTF-8");

		std::wstring wide(imagePath);
		int bufferSize = WideCharToMultiByte(CP_OEMCP, 0, wide.c_str(), -1, (char*)NULL, 0, NULL, NULL);

		auto* cpMultiByte = new CHAR[bufferSize];
		WideCharToMultiByte(CP_OEMCP, 0, wide.c_str(), -1, cpMultiByte, bufferSize, NULL, NULL);
		std::string imagePathStr(cpMultiByte, cpMultiByte + bufferSize - 1);
		delete[] cpMultiByte;

		// 変換結果を返す
		return imagePathStr;
	}
}